using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FireSlop.Cats;
using FireSlop.Firefighter;
using FireSlop.Interaction;
using FireSlop.MissionHud;
using FireSlop.Tools;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Zenject;
using Object = UnityEngine.Object;

namespace FireSlop.Verification
{
    public static class CatPatrolVerification
    {
        public static async Task<string> CheckRoamingAndFire()
        {
            Require(Application.isPlaying, "Start a fresh Play Mode run.");
            var context = Object.FindObjectsByType<SceneContext>().Single(c => c.gameObject.scene.name == "Main");
            var container = context.Container;
            var input = container.Resolve<MissionInputModel>();
            var tools = container.Resolve<ToolSelectionModel>();
            tools.Select(tools.Tools.First(t => t.Action == ToolAction.Hammer).Id);
            var cats = Object.FindObjectsByType<CatController>().OrderBy(c => c.ContentId).ToArray();
            Require(cats.Length == 2, "There must be exactly two authored cats.");
            var start = cats.Select(c => c.transform.position).ToArray();
            var maximumTravel = new float[cats.Length];
            var areas = cats.Select(c => (CatPatrolArea)new SerializedObject(c.GetComponent<CatPatrolController>())
                .FindProperty("_area").objectReferenceValue).ToArray();
            for (var index = 0; index < cats.Length; index++)
            {
                var agent = cats[index].GetComponent<NavMeshAgent>();
                Require(agent.isOnNavMesh, "Each cat must start on its apartment NavMesh.");
                Require(cats[index].GetComponent<Rigidbody>().isKinematic,
                    "Navigation must not compete with a dynamic Rigidbody.");
                Require(areas[index].IsValid && areas[index].Contains(start[index]),
                    "Each authored spawn must be inside a baked apartment.");
                for (var point = 0; point < areas[index].PointCount; point++)
                {
                    var path = new NavMeshPath();
                    Require(agent.CalculatePath(areas[index].GetPoint(point), path) &&
                        path.status == NavMeshPathStatus.PathComplete, "Every patrol point must be reachable.");
                }
            }

            var until = Time.realtimeSinceStartup + 4f;
            while (Time.realtimeSinceStartup < until)
            {
                for (var index = 0; index < cats.Length; index++)
                {
                    Require(areas[index].Contains(cats[index].transform.position), "A cat must stay in its apartment.");
                    maximumTravel[index] = Mathf.Max(maximumTravel[index],
                        Vector3.Distance(start[index], cats[index].transform.position));
                }
                await Task.Yield();
            }
            Require(maximumTravel.All(d => d > 0.4f), "Both cats must visibly move instead of sitting still.");
            input.SetSuspended(true);
            await WaitSeconds(0.1f);
            var paused = cats.Select(c => c.transform.position).ToArray();
            await WaitSeconds(0.25f);
            Require(cats.Select((c, index) => Vector3.Distance(c.transform.position, paused[index]) < 0.01f).All(v => v),
                "Pause must stop both agents.");
            input.SetSuspended(false);

            var first = cats[0];
            var localContainer = first.GetComponent<GameObjectContext>().Container;
            var state = localContainer.Resolve<CatModel>();
            var patrol = localContainer.Resolve<CatPatrolModel>();
            var firePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Props/FireSource.prefab");
            var fireSettings = (WaterFireConfigSO)new SerializedObject(firePrefab.GetComponent<WaterFireTarget>())
                .FindProperty("_settings").objectReferenceValue;
            var fires = new List<GameObject>();
            var player = Object.FindAnyObjectByType<FirefighterController>();
            var character = player.GetComponent<CharacterController>();
            var oldPosition = player.transform.position;
            var oldPlayerEnabled = player.enabled;
            var oldCharacterEnabled = character.enabled;
            var surroundedPosition = first.transform.position;
            try
            {
                for (var index = 0; index < 8; index++)
                {
                    var angle = index * Mathf.PI * 0.25f;
                    var position = surroundedPosition + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 0.85f;
                    var fireModelType = typeof(WaterFireTarget).Assembly.GetType("FireSlop.Interaction.WaterFireModel");
                    var fireModel = Activator.CreateInstance(fireModelType, new object[]
                    {
                        fireSettings.SecondsToExtinguish, fireSettings.InitialIntensity, fireSettings.PassageOpenIntensity, true
                    });
                    var fire = container.InstantiatePrefabForComponent<WaterFireTarget>(firePrefab, position,
                        Quaternion.identity, null, new object[] { fireModel });
                    fires.Add(fire.gameObject);
                }
                Physics.SyncTransforms();
                player.enabled = false;
                character.enabled = false;
                player.transform.position = surroundedPosition + Vector3.forward * 0.6f;
                await WaitSeconds(0.15f);
                var stoppedPosition = first.transform.position;
                await WaitSeconds(2.1f);
                Require(patrol.IsFireTrapped, "Surrounding fire must mark the cat trapped.");
                Require(state.State == CatState.Idle, "A trapped cat must neither warn nor attack a nearby player.");
                Require(Vector3.Distance(stoppedPosition, first.transform.position) < 0.01f,
                    "A trapped cat must stay still instead of entering fire.");
                player.transform.position = oldPosition;
                foreach (var fireRoot in fires)
                {
                    var fire = fireRoot.GetComponent<WaterFireTarget>();
                    var water = new WaterImpact(default, Vector3.forward, 100f, 0f);
                    fire.ApplyWater(water);
                    Require(!fire.RequestsAutoSpray, "The test ring must actually be extinguished.");
                }
                Physics.SyncTransforms();
                await WaitSeconds(2.2f);
                Require(!patrol.IsFireTrapped, "Extinguishing the ring must release the waiting state.");
                Require(Vector3.Distance(stoppedPosition, first.transform.position) > 0.1f,
                    "The cat must resume moving after a safe route reopens.");
                return "PASS: two apartment NavMeshes, all route points reachable, both cats moving, bounded routes, " +
                    "kinematic navigation, pause, surrounding fire stops movement and attack, extinguishing resumes patrol.";
            }
            finally
            {
                input.SetSuspended(false);
                player.transform.position = oldPosition;
                player.enabled = oldPlayerEnabled;
                character.enabled = oldCharacterEnabled;
                foreach (var fire in fires)
                {
                    Object.Destroy(fire);
                }
            }
        }

        private static async Task WaitSeconds(float seconds)
        {
            var until = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < until)
            {
                await Task.Yield();
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
