using System;
using System.IO;
using FireSlop.Interaction;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace FireSlop.Verification
{
    public static class FurniturePhysicsVerification
    {
        public static string Run()
        {
            if (!Application.isPlaying)
            {
                throw new InvalidOperationException("Furniture physics verification requires Play Mode.");
            }

            var names = new[] { "Fridge", "Stove", "DiningChair", "CoatRack", "ShoeCabinet" };
            var scene = SceneManager.CreateScene("FurniturePhysicsVerification", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
            var physics = scene.GetPhysicsScene();
            var results = new JArray();
            try
            {
                for (var index = 0; index < names.Length; index++)
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Props/Furniture/" + names[index] + ".prefab");
                    var first = Object.Instantiate(prefab, new Vector3(index * 4f, 10f, 0f), Quaternion.identity);
                    var second = Object.Instantiate(prefab, new Vector3(index * 4f, 10f, 4f), Quaternion.identity);
                    SceneManager.MoveGameObjectToScene(first, scene);
                    SceneManager.MoveGameObjectToScene(second, scene);
                    var body = first.GetComponent<Rigidbody>();
                    var otherBody = second.GetComponent<Rigidbody>();
                    var settings = (FurniturePhysicsSO)new SerializedObject(first.GetComponent<Movable>())
                        .FindProperty("_settings").objectReferenceValue;
                    Require(body.mass == settings.Mass && body.linearDamping == settings.LinearDamping,
                        names[index] + " must initialize its physics from the SO in Awake.");
                    var obstacle = first.GetComponent<NavMeshObstacle>();
                    Require(obstacle.carving && Mathf.Abs(obstacle.carvingTimeToStationary - 0.3f) < 0.001f,
                        names[index] + " must initialize navigation from shared settings.");
                    obstacle.enabled = false;
                    second.GetComponent<NavMeshObstacle>().enabled = false;
                    body.useGravity = false;
                    otherBody.useGravity = false;
                    Physics.SyncTransforms();
                    var offset = names[index] == "DiningChair" ? new Vector3(0.195f, 0.2f, 2f) : new Vector3(0f, 0.4f, 2f);
                    Require(physics.Raycast(first.transform.position + offset, Vector3.back, out var hit, 3f, 1 << 10),
                        names[index] + " must be hit through its authored collision geometry.");
                    Require(hit.rigidbody == body, names[index] + " hit must resolve to its own body.");
                    var target = hit.collider.GetComponentInParent<IWaterTarget>();
                    Require(target != null, names[index] + " must use the existing water receiver contract.");
                    var initial = first.transform.position;
                    var untouched = second.transform.position;
                    var impact = new WaterImpact(hit, Vector3.back, 0.1f, body.mass * 2f);
                    target.ApplyWater(impact);
                    for (var step = 0; step < 12; step++)
                    {
                        physics.Simulate(1f / 60f);
                    }

                    var moved = Vector3.Distance(initial, first.transform.position);
                    Require(moved > 0.1f, names[index] + " must move under water impulse.");
                    Require(Vector3.Distance(untouched, second.transform.position) < 0.001f,
                        names[index] + " must not move another instance.");
                    results.Add(new JObject { ["name"] = names[index], ["movedMeters"] = moved,
                        ["massFromSO"] = true, ["secondInstanceUnchanged"] = true, ["passed"] = true });
                }
            }
            finally
            {
                foreach (var root in scene.GetRootGameObjects())
                {
                    Object.DestroyImmediate(root);
                }

                SceneManager.UnloadSceneAsync(scene);
            }

            File.WriteAllText("Documentation/Art/ModelProduction/furniture-physics-validation.json", results.ToString() + "\n");
            return "All five furniture types initialize correctly, receive water impulses and preserve independent instances.";
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
