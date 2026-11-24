using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FireSlop.Firefighter;
using FireSlop.Drone;
using FireSlop.Interaction;
using FireSlop.MissionHud;
using FireSlop.Tools;

using UnityEditor;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace FireSlop.Verification
{
    public static class ThrowableBrickVerification
    {
        public static string CheckAuthoring()
        {
            var bricks = Object.FindObjectsByType<ThrowableBrick>();
            Require(bricks.Length == 2 && bricks.Select(x => x.ContentId).Distinct().Count() == 2,
                "Main needs two distinct authored bricks.");
            foreach (var brick in bricks)
            {
                Require(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(brick) ==
                    "Assets/Prefabs/Props/ThrowableBrick.prefab", "Brick prefab connection must survive saving.");
                Require(brick.gameObject.activeInHierarchy && brick.GetComponentInChildren<MeshRenderer>().enabled,
                    "Bricks must be visible in Edit Mode.");
                Require(brick.GetComponent<Rigidbody>().collisionDetectionMode == CollisionDetectionMode.ContinuousDynamic,
                    "Fast bricks need continuous collision detection.");
                Require(brick.GetComponentInChildren<LineRenderer>(true) == null, "World pickups must have no arcs.");
            }

            var brickArc = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Characters/Firefighter/Firefighter.prefab")
                .GetComponentInChildren<ThrowArcView>(true);
            var signalArc = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Props/SignalThrow.prefab")
                .GetComponentInChildren<ThrowArcView>(true);
            var sharedArc = PrefabUtility.GetCorrespondingObjectFromOriginalSource(brickArc);
            Require(sharedArc != null && AssetDatabase.GetAssetPath(sharedArc) ==
                "Assets/Prefabs/Props/ThrowArc.prefab" && sharedArc ==
                PrefabUtility.GetCorrespondingObjectFromOriginalSource(signalArc),
                "Brick and signal must use the same nested arc prefab asset.");
            return "PASS: two distinct connected, visible bricks, continuous physics, hidden arcs and shared nested arc prefab.";
        }

        public static async Task<string> CheckRun()
        {
            var container = Object.FindObjectsByType<SceneContext>()
                .Single(x => x.gameObject.scene.name == "Main").Container;
            var carrier = container.Resolve<RescueController>();
            var interaction = container.Resolve<InteractionModel>();
            var input = container.Resolve<MissionInputModel>();
            var tools = container.Resolve<ToolSelectionModel>();
            var tool = tools.Tools[3];
            Require(tool.Action == ToolAction.Brick && tool.SelectionKey == UnityEngine.InputSystem.Key.Digit4 &&
                !tools.IsAvailable(tool.Id), "Fourth slot must start empty.");
            var camera = container.Resolve<Camera>();
            var pose = container.Resolve<ThrowPoseView>();
            var player = camera.GetComponentInParent<FirefighterController>();
            player.enabled = false;
            player.GetComponent<CharacterController>().enabled = false;
            var bricks = Object.FindObjectsByType<ThrowableBrick>().OrderBy(x => x.ContentId).ToArray();
            var brick = bricks[1];
            var body = brick.GetComponent<Rigidbody>();
            var arc = player.GetComponentInChildren<ThrowArcView>(true).GetComponent<LineRenderer>();
            var held = pose.HoldPoint.GetComponentsInChildren<EquippedToolModelView>(true).Single(x => x.ToolId == tool.Id);
            var context = Object.FindAnyObjectByType<InteractionController>();
            var resolve = typeof(InteractionController).GetMethod("ResolveTarget", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var pickup in bricks)
            {
                var position = pickup.transform.position;
                Aim(camera, position + Vector3.up * 1.6f + Vector3.back * 0.5f, position);
                Require(ReferenceEquals(resolve.Invoke(context, null), pickup), "The crosshair must resolve the pickup.");
                interaction.Interact();
                Require(!pickup.gameObject.activeSelf && !interaction.IsCarrying && carrier.Carried == null,
                    "Pickup must store the world object and keep rescue hands free.");
                pickup.Interact();
            }

            Require(tools.TryGetQuantity(tool.Id, out var count) && count == 2 && tools.SelectedId == tool.Id &&
                held.gameObject.activeInHierarchy && held.transform.localPosition == Vector3.zero &&
                held.GetComponentInChildren<Rigidbody>() == null, "Stack two pickups and equip one visual.");
            var buttons = Object.FindObjectsByType<ToolButtonView>(FindObjectsInactive.Include);
            var button = buttons.Single(x => x.GetComponentsInChildren<TMPro.TMP_Text>(true)
                .Any(t => t.text == "Brick ×2"));
            tools.Select(tools.Tools[0].Id);
            Require(!held.gameObject.activeSelf, "Switching tools must hide the held brick.");
            button.GetComponentInChildren<UnityEngine.UI.Button>(true).onClick.Invoke();
            Require(tools.SelectedId == tool.Id && held.gameObject.activeSelf, "HUD button must re-equip the stack.");
            var section = Object.FindObjectsByType<WindowSectionView>().Single(x => x.name == "Upper_Intact" &&
                Mathf.Abs(x.transform.position.x + 18.93f) < 0.1f && x.transform.position.z > 5f &&
                x.transform.position.y < 1f);
            var pane = section.GetComponent<Collider>();
            var target = pane.bounds.center;
            Aim(camera, new Vector3(target.x - 0.2f, 0.8f, target.z + 4f),
                new Vector3(target.x - 0.2f, 0.8f, target.z));
            await Wait(0.1f);
            var start = pose.LaunchPosition;
            var velocity = pose.LaunchVelocity;
            Require(arc.enabled && arc.positionCount > 2 && Vector3.Distance(arc.GetPosition(0), start) < 0.001f,
                "The visible arc must use the actual release origin.");
            var signalArc = Object.FindAnyObjectByType<SignalThrowController>()
                .GetComponentInChildren<ThrowArcView>(true);
            signalArc.Trace(pose, brick.transform);
            var signalLine = signalArc.GetComponent<LineRenderer>();
            Require(signalLine.positionCount == arc.positionCount && signalLine.sharedMaterial == arc.sharedMaterial &&
                signalLine.widthMultiplier == arc.widthMultiplier,
                "Both items must use the same arc renderer styling and sampling.");
            for (var i = 0; i < arc.positionCount; i++)
            {
                Require(Vector3.Distance(signalLine.GetPosition(i), arc.GetPosition(i)) < 0.001f,
                    "All trajectory points must match for identical aim.");
            }

            var expected = start + velocity * 0.05f + Physics.gravity * (0.5f * 0.05f * 0.05f);
            Require(Vector3.Distance(arc.GetPosition(1), expected) < 0.001f,
                "Arc samples must follow the actual launch velocity and gravity.");
            input.SetSuspended(true);
            interaction.BeginPrimary(PrimaryInputSource.Touch);
            interaction.EndPrimary(PrimaryInputSource.Touch);
            await Wait(0.05f);
            Require(!brick.gameObject.activeSelf && !arc.enabled, "Pause must block throwing and hide the arc.");
            input.SetSuspended(false);

            Aim(camera, new Vector3(target.x, target.y, target.z + 0.2f), target);
            interaction.BeginPrimary(PrimaryInputSource.Touch);
            interaction.EndPrimary(PrimaryInputSource.Touch);
            Require(!brick.gameObject.activeSelf, "An obstructed release must preserve the carried brick.");
            Aim(camera, new Vector3(target.x - 0.2f, 0.8f, target.z + 4f),
                new Vector3(target.x - 0.2f, 0.8f, target.z));
            velocity = pose.LaunchVelocity;
            interaction.BeginPrimary(PrimaryInputSource.Touch);
            interaction.EndPrimary(PrimaryInputSource.Touch);
            Require(carrier.Carried == null && !interaction.IsCarrying && !body.isKinematic &&
                Vector3.Distance(body.linearVelocity, velocity) < 0.001f,
                "Shared touch action must launch the same brick with the predicted velocity.");
            await Wait(0.5f);
            Require(!pane.enabled && brick.gameObject.activeSelf && arc.enabled && tools.TryGetQuantity(tool.Id, out count) && count == 1,
                "A physical collision must break glass and preserve the reusable brick.");
            Require(!Physics.Raycast(new Vector3(target.x, target.y, target.z + 0.5f), Vector3.back,
                out var hit, 0.8f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) ||
                hit.collider.GetComponentInParent<WindowSectionView>() == null,
                "The broken pane must open a clear ray path for the hose.");
            Aim(camera, new Vector3(-24f, 0.6f, 10f), new Vector3(-24f, -1.3f, 11f));
            interaction.BeginPrimary(PrimaryInputSource.Mouse);
            interaction.EndPrimary(PrimaryInputSource.Mouse);
            Require(!tools.IsAvailable(tool.Id) && !button.gameObject.activeSelf && !held.gameObject.activeSelf &&
                bricks[0].gameObject.activeSelf && tools.SelectedId == tools.Tools[0].Id,
                "The final throw must empty the slot and restore the default tool.");
            await Wait(0.3f);
            brick.Interact();
            Require(tools.TryGetQuantity(tool.Id, out count) && count == 1 && held.gameObject.activeSelf &&
                tools.IsAvailable(tools.Tools.Single(x => x.Action == ToolAction.SignalCanister).Id),
                "Recover the same brick with an independent canister charge.");
            return "PASS: two aimed pickups, duplicate guards, stack and HUD selection, static held visual, common arc, " +
                "blocked and paused throws, touch window hit, mouse depletion, default tool and recovery.";
        }

        public static async Task<string> CheckSignal()
        {
            var container = Object.FindObjectsByType<SceneContext>()
                .Single(x => x.gameObject.scene.name == "Main").Container;
            var interaction = container.Resolve<InteractionModel>();
            var tools = container.Resolve<ToolSelectionModel>();
            var camera = container.Resolve<Camera>();
            var pose = container.Resolve<ThrowPoseView>();
            var controller = Object.FindAnyObjectByType<SignalThrowController>();
            var trajectory = controller.GetComponentInChildren<ThrowArcView>(true);
            var trace = typeof(SignalThrowController).GetMethod("Trace", BindingFlags.Instance | BindingFlags.NonPublic);
            var focus = typeof(SignalThrowController).GetMethod("OnApplicationFocus",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var signal = typeof(SignalThrowController).GetField("_signal", BindingFlags.Instance | BindingFlags.NonPublic);
            var tool = tools.Tools.Single(x => x.Action == ToolAction.SignalCanister);
            tools.Select(tool.Id);
            tools.TryGetQuantity(tool.Id, out var before);
            Require(before == 1 && !interaction.IsCarrying, "Start with one canister and free hands.");
            Aim(camera, new Vector3(-21f, 0.8f, 9f), new Vector3(-21f, 20f, 9f));
            interaction.BeginPrimary(PrimaryInputSource.Touch);
            interaction.EndPrimary(PrimaryInputSource.Touch);
            tools.TryGetQuantity(tool.Id, out var afterInvalid);
            Require(afterInvalid == before && signal.GetValue(controller) == null,
                "An invalid trajectory must preserve the canister.");
            var found = false;
            for (var yaw = 0; yaw < 360 && !found; yaw += 30)
            {
                for (var pitch = 20; pitch <= 60 && !found; pitch += 20)
                {
                    var direction = Quaternion.Euler(pitch, yaw, 0f) * Vector3.forward;
                    Aim(camera, new Vector3(-21f, 0.8f, 9f), new Vector3(-21f, 0.8f, 9f) + direction);
                    found = (bool)trace.Invoke(controller, null);
                }
            }

            Require(found, "A clear courtyard landing must remain valid with shared ballistics.");
            focus.Invoke(controller, new object[] { false });
            interaction.BeginPrimary(PrimaryInputSource.Touch);
            interaction.EndPrimary(PrimaryInputSource.Touch);
            tools.TryGetQuantity(tool.Id, out var afterFocusLoss);
            Require(afterFocusLoss == before, "Focus loss must block signal release.");
            focus.Invoke(controller, new object[] { true });
            var launch = pose.LaunchPosition;
            interaction.BeginPrimary(PrimaryInputSource.Touch);
            interaction.EndPrimary(PrimaryInputSource.Touch);
            tools.TryGetQuantity(tool.Id, out var spent);
            var projectile = (ParticleSystem)signal.GetValue(controller);
            Require(spent == 0 && projectile != null && Vector3.Distance(projectile.transform.position, launch) < 0.001f,
                "A valid shared input must spend one canister and use the common launch origin.");
            interaction.BeginPrimary(PrimaryInputSource.Touch);
            interaction.EndPrimary(PrimaryInputSource.Touch);
            Require((ParticleSystem)signal.GetValue(controller) == projectile,
                "Repeated input must not create another projectile.");
            var startPosition = projectile.transform.position;
            await Wait(0.15f);
            Require(Vector3.Distance(projectile.transform.position, startPosition) > 0.01f,
                "The signal must advance along the shared trajectory.");
            await Wait(trajectory.Path.Duration + 0.2f);
            Require(projectile.isPlaying && Vector3.Distance(projectile.transform.position,
                trajectory.Path.PositionAt(trajectory.Path.Duration)) < 0.001f,
                "The signal must arrive at the shared predicted endpoint and emit smoke.");
            return "PASS: invalid landing and focus preserve charge; valid touch spends once, common origin, " +
                "single projectile, shared flight and smoke at predicted endpoint.";
        }

        public static async Task<string> CheckResidentRegression()
        {
            var container = Object.FindObjectsByType<SceneContext>()
                .Single(x => x.gameObject.scene.name == "Main").Container;
            var carrier = container.Resolve<RescueController>();
            var interaction = container.Resolve<InteractionModel>();
            var camera = container.Resolve<Camera>();
            if (carrier.Carried != null)
            {
                carrier.Deliver(carrier.Carried, new Vector3(-20f, -1.36f, 7.5f));
            }

            var resident = container.ResolveAll<ResidentView>()[0];
            var cabinet = (Openable)typeof(ResidentView).GetField("_cabinet",
                BindingFlags.Instance | BindingFlags.NonPublic).GetValue(resident);
            if (!cabinet.IsOpen)
            {
                cabinet.GetComponent<CabinetContextTarget>().Interact();
            }

            await Wait(0.8f);
            Require(cabinet.IsFullyOpen, "Open the resident cabinet before pickup.");
            resident.Interact();
            var shoulder = (Transform)new SerializedObject(carrier).FindProperty("_shoulder").objectReferenceValue;
            Require(carrier.Carried == resident && resident.transform.parent == shoulder &&
                resident.transform.parent != container.Resolve<ThrowPoseView>().HoldPoint,
                "Residents must retain shoulder attachment after the shared throw refactor.");
            Aim(camera, new Vector3(-24f, 0.8f, 9f), new Vector3(-24f, 0.8f, 12f));
            var config = AssetDatabase.LoadAssetAtPath<InteractionConfigSO>(
                AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:InteractionConfigSO")[0]));
            var velocity = camera.transform.forward * config.ThrowSpeed;
            interaction.BeginPrimary(PrimaryInputSource.Touch);
            interaction.EndPrimary(PrimaryInputSource.Touch);
            var body = resident.GetComponent<Rigidbody>();
            Require(carrier.Carried == null && !body.isKinematic &&
                Vector3.Distance(body.linearVelocity, velocity) < 0.001f,
                "The original resident throw path must remain functional.");
            return "PASS: resident cabinet, original shoulder, release clearance and original throw velocity.";
        }

        private static void Aim(Camera camera, Vector3 eye, Vector3 target)
        {
            camera.transform.position = eye;
            camera.transform.rotation = Quaternion.LookRotation(target - eye);
            Physics.SyncTransforms();
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
