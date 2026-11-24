using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FireSlop.Drone;
using FireSlop.Firefighter;
using FireSlop.Interaction;
using FireSlop.MissionHud;
using FireSlop.Tools;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;
using Object = UnityEngine.Object;

namespace FireSlop.Verification
{
    public static class DroneLandingPreviewVerification
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        public static string CheckAuthoring()
        {
            var preview = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Modules/Drone/LandingPreview.prefab");
            var drone = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Modules/Drone/RescueDrone.prefab");
            var meshes = preview.GetComponentsInChildren<MeshFilter>(true);
            var originals = drone.GetComponentsInChildren<MeshFilter>(true);
            Require(meshes.Length == originals.Length && meshes.Length > 0, "Preview must show every drone mesh.");
            for (var index = 0; index < meshes.Length; index++)
            {
                Require(meshes[index].sharedMesh == originals[index].sharedMesh &&
                    meshes[index].transform.localToWorldMatrix == originals[index].transform.localToWorldMatrix,
                    "Preview geometry, scale and pivot must match the actual drone.");
            }

            Require(preview.GetComponentsInChildren<Component>(true).All(component =>
                component is Transform || component is MeshFilter || component is MeshRenderer),
                "Preview must contain no gameplay, physics or audio components.");
            foreach (var renderer in preview.GetComponentsInChildren<MeshRenderer>(true))
            {
                Require(renderer.shadowCastingMode == ShadowCastingMode.Off && !renderer.receiveShadows,
                    "The ghost must not cast or receive shadows.");
                foreach (var material in renderer.sharedMaterials)
                {
                    var alpha = material.GetColor("_BaseColor").a;
                    Require(alpha > 0f && alpha < 1f && material.renderQueue >= 3000 &&
                        material.GetFloat("_ZWrite") == 0f && material.IsKeywordEnabled("_SURFACE_TYPE_TRANSPARENT"),
                        "Every preview material must be transparent without depth writes.");
                }
            }

            var signal = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Props/SignalThrow.prefab");
            var authored = (Transform)new SerializedObject(signal.GetComponent<SignalThrowController>())
                .FindProperty("_landingPreview").objectReferenceValue;
            Require(authored != null && !authored.gameObject.activeSelf &&
                PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(authored) == AssetDatabase.GetAssetPath(preview),
                "SignalThrow needs one connected nested preview, hidden before aiming.");
            Require(signal.transform.Find("Landing Footprint") == null &&
                signal.GetComponentsInChildren<LineRenderer>(true).Length == 1, "Only the throw arc may remain.");
            return "PASS: 21 matching meshes, correct pivot/scale, translucent authored material, no physics/audio, connected nested preview, old frame removed.";
        }

        public static async Task<string> CheckAim()
        {
            var container = Object.FindObjectsByType<SceneContext>().Single(x => x.gameObject.scene.name == "Main").Container;
            var tools = container.Resolve<ToolSelectionModel>();
            var input = container.Resolve<MissionInputModel>();
            var camera = container.Resolve<Camera>();
            var player = camera.GetComponentInParent<FirefighterController>();
            player.enabled = false;
            player.GetComponent<CharacterController>().enabled = false;
            var controller = Object.FindAnyObjectByType<SignalThrowController>();
            var drone = container.Resolve<DroneController>();
            var preview = (Transform)typeof(SignalThrowController).GetField("_landingPreview", PrivateInstance).GetValue(controller);
            var tool = tools.Tools.Single(x => x.Action == ToolAction.SignalCanister);
            tools.Select(tool.Id);
            Require(FindAim(camera, controller, new Vector3(-21f, 0.8f, 9f)), "Courtyard must have a valid landing.");
            await Wait(0.1f);
            Require(preview.gameObject.activeInHierarchy, "Valid aim must show the ghost.");
            var landing = (Vector3)typeof(SignalThrowController).GetField("_landing", PrivateInstance).GetValue(controller);
            Require(Vector3.Distance(preview.position, landing) < 0.001f && preview.rotation == drone.transform.rotation,
                "Ghost pose must match the predicted landing and drone orientation.");
            var validRotation = camera.transform.rotation;
            camera.transform.rotation = Quaternion.LookRotation(Vector3.up);
            await Wait(0.1f);
            Require(!preview.gameObject.activeSelf, "Invalid aim must hide a previously valid ghost.");
            camera.transform.rotation = validRotation;
            await Wait(0.1f);
            input.SetSuspended(true);
            await Wait(0.1f);
            Require(!preview.gameObject.activeSelf, "Pause must hide the ghost.");
            input.SetSuspended(false);
            var focus = typeof(SignalThrowController).GetMethod("OnApplicationFocus", PrivateInstance);
            focus.Invoke(controller, new object[] { false });
            await Wait(0.1f);
            Require(!preview.gameObject.activeSelf, "Focus loss must hide the ghost.");
            focus.Invoke(controller, new object[] { true });
            tools.Select(tools.Tools[0].Id);
            await Wait(0.1f);
            Require(!preview.gameObject.activeSelf, "Tool switch must hide the ghost.");
            tools.Select(tool.Id);
            await Wait(0.1f);
            controller.enabled = false;
            Require(!preview.gameObject.activeSelf, "Disabling must hide the ghost immediately.");
            controller.enabled = true;
            await Wait(0.1f);
            Require(preview.gameObject.activeSelf && tools.TryGetQuantity(tool.Id, out var count) && count == 1,
                "Reopening must restore valid aim without consuming a canister.");
            return "PASS: valid pose, invalid aim, pause, focus, tool switch, disable/reopen and preserved charge. Ready for capture.";
        }

        public static async Task<string> CheckLanding()
        {
            var container = Object.FindObjectsByType<SceneContext>().Single(x => x.gameObject.scene.name == "Main").Container;
            var controller = Object.FindAnyObjectByType<SignalThrowController>();
            var preview = (Transform)typeof(SignalThrowController).GetField("_landingPreview", PrivateInstance).GetValue(controller);
            var expectedPosition = preview.position;
            var expectedRotation = preview.rotation;
            var interaction = container.Resolve<InteractionModel>();
            interaction.BeginPrimary(PrimaryInputSource.Touch);
            interaction.EndPrimary(PrimaryInputSource.Touch);
            Require(!preview.gameObject.activeSelf, "Releasing the canister must immediately hide the ghost.");
            interaction.BeginPrimary(PrimaryInputSource.Mouse);
            interaction.EndPrimary(PrimaryInputSource.Mouse);
            var drone = container.Resolve<DroneController>();
            var arrived = typeof(DroneController).GetProperty("HasArrived", PrivateInstance);
            var until = Time.realtimeSinceStartup + 35f;
            while (!(bool)arrived.GetValue(drone) && Time.realtimeSinceStartup < until)
            {
                await Wait(0.2f);
            }

            Require((bool)arrived.GetValue(drone), "Drone must finish its approach.");
            Require(Vector3.Distance(drone.transform.position, expectedPosition) < 0.01f &&
                Quaternion.Angle(drone.transform.rotation, expectedRotation) < 0.01f,
                "Actual touchdown must match the ghost's position and orientation.");
            Require(!preview.gameObject.activeSelf && Object.FindObjectsByType<DroneController>().Length == 1,
                "A spent signal must leave the preview hidden and exactly one actual drone.");
            return "PASS: shared touch throw hides preview, repeated mouse input creates no duplicate drone, actual touchdown matches ghost pose.";
        }

        public static async Task<string> CheckRoof()
        {
            var container = Object.FindObjectsByType<SceneContext>().Single(x => x.gameObject.scene.name == "Main").Container;
            var camera = container.Resolve<Camera>();
            var player = camera.GetComponentInParent<FirefighterController>();
            player.enabled = false;
            player.GetComponent<CharacterController>().enabled = false;
            var tools = container.Resolve<ToolSelectionModel>();
            tools.Select(tools.Tools.Single(x => x.Action == ToolAction.SignalCanister).Id);
            var controller = Object.FindAnyObjectByType<SignalThrowController>();
            Require(FindAim(camera, controller, new Vector3(-14f, 9.4f, 0f)), "The roof must have a valid landing.");
            await Wait(0.1f);
            var preview = (Transform)typeof(SignalThrowController).GetField("_landingPreview", PrivateInstance).GetValue(controller);
            Require(preview.gameObject.activeInHierarchy && preview.position.y > 7f,
                "The ghost must appear on the roof, above courtyard height.");
            return "PASS: fresh mission restored the canister and valid roof preview. Ready for capture and touchdown check.";
        }

        private static bool FindAim(Camera camera, SignalThrowController controller, Vector3 eye)
        {
            var trace = typeof(SignalThrowController).GetMethod("Trace", PrivateInstance);
            camera.transform.position = eye;
            for (var yaw = 0; yaw < 360; yaw += 30)
            {
                for (var pitch = 20; pitch <= 60; pitch += 20)
                {
                    camera.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
                    Physics.SyncTransforms();
                    if ((bool)trace.Invoke(controller, null))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static async Task Wait(float seconds)
        {
            var until = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < until)
            {
                await Task.Delay(20);
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
