using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FireSlop.Cats;
using FireSlop.Firefighter;
using FireSlop.Interaction;
using FireSlop.Mission;
using FireSlop.MissionHud;
using FireSlop.Tools;
using UnityEditor;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace FireSlop.Verification
{
    public static class CatPlayVerification
    {
        public static async Task<string> CheckWaterAndRescue()
        {
            var container = FindContainer();
            var camera = container.Resolve<Camera>();
            var input = container.Resolve<InteractionModel>();
            var tools = container.Resolve<ToolSelectionModel>();
            var mission = container.Resolve<MissionModel>();
            var player = Object.FindAnyObjectByType<FirefighterController>();
            var character = player.GetComponent<CharacterController>();
            var originalPosition = player.transform.position;
            var originalRotation = camera.transform.localRotation;
            var cat = FindCat("cat-ground-hall");
            var other = FindCat("cat-upstairs-room");
            var model = cat.GetComponent<GameObjectContext>().Container.Resolve<CatModel>();
            var otherModel = other.GetComponent<GameObjectContext>().Container.Resolve<CatModel>();
            var view = cat.GetComponent<CatView>();
            var carry = cat.GetComponent<CatCarryable>();
            cat.GetComponent<CatPatrolController>().enabled = false;
            cat.GetComponent<GameObjectContext>().Container.Resolve<CatPatrolModel>().SetFireSafety(false, 0f);
            var interaction = Object.FindAnyObjectByType<InteractionController>();
            GameObject blocker = null;
            player.enabled = false;
            character.enabled = false;
            try
            {
                Require(model.State == CatState.Idle && otherModel.State == CatState.Idle,
                    "A fresh run must have two independent idle cats.");
                Require(cat.transform.position.x > -13.8f && cat.transform.position.z < -0.8f,
                    "The cat must start inside the authored apartment.");
                tools.Select(tools.Tools.First(t => t.Action == ToolAction.Hammer).Id);
                player.transform.position = new Vector3(-24f, -0.76f, 10f);
                cat.transform.position = new Vector3(-24f, -1.21f, 8.5f);
                Physics.SyncTransforms();
                camera.transform.LookAt(view.NoticePosition);
                await WaitSeconds(0.2f);
                Require(model.State == CatState.Warning && cat.GetComponent<AudioSource>().isPlaying,
                    "A visible nearby cat must hiss before jumping.");
                var warning = cat.GetComponentInChildren<CatWarningView>(true);
                Require(warning.IsVisible, "A warning must show the three exclamation marks.");
                await WaitSeconds(1f);
                Require(model.State == CatState.Warning, "The new three-second warning must not jump early.");
                var towardPlayer = Vector3.ProjectOnPlane(camera.transform.position - cat.transform.position,
                    Vector3.up).normalized;
                Require(Vector3.Dot(cat.transform.forward, towardPlayer) > 0.95f,
                    "A warning cat must turn toward the nearby player.");
                player.transform.position += Vector3.back * 6f;
                await WaitSeconds(0.15f);
                Require(model.State == CatState.Idle && !warning.IsVisible && !cat.GetComponent<AudioSource>().isPlaying,
                    "Backing away must immediately cancel the hiss and warning markers.");
                player.transform.position = new Vector3(-24f, -0.76f, 10f);
                camera.transform.LookAt(view.NoticePosition);
                blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                blocker.name = "Cat verification sight blocker";
                blocker.transform.position = Vector3.Lerp(view.NoticePosition, camera.transform.position, 0.5f);
                blocker.transform.localScale = new Vector3(2f, 3f, 0.15f);
                Physics.SyncTransforms();
                await WaitSeconds(0.15f);
                Require(model.State == CatState.Idle, "An obstruction must cancel the warning.");
                Object.Destroy(blocker);
                blocker = null;
                await WaitSeconds(0.1f);
                tools.Select(tools.Tools.First(t => t.Action == ToolAction.Hose).Id);
                AimHose(camera, view.NoticePosition);
                input.BeginPrimary(PrimaryInputSource.Touch);
                await WaitSeconds(0.8f);
                input.EndPrimary(PrimaryInputSource.Touch);
                Require(model.State == CatState.Settled && model.WaterSeconds > 0f,
                    "Real hose packets must calm the cat before its jump.");
                Require(otherModel.State == CatState.Idle, "Water must not calm the other cat.");
                Require(!warning.IsVisible && !cat.GetComponent<AudioSource>().isPlaying,
                    "Water calming must hide the warning and stop the hiss.");
                Require(cat.transform.Find("Calm Sound").GetComponent<AudioSource>().isPlaying,
                    "Calming with water must play the purr-meow cue.");
                camera.transform.LookAt(view.NoticePosition);
                Physics.SyncTransforms();
                RefreshContext(interaction);
                Require(input.ContextPrompt.ActionId == HudActionId.PickUp,
                    "A calmed cat must advertise pickup through the normal aim query.");
                input.Interact();
                Require(input.CarriedContentId == carry.ContentId, "The context action must pick up the cat.");
                var zone = Object.FindAnyObjectByType<RescueZoneView>();
                var volume = zone.GetComponent<BoxCollider>();
                player.transform.position += volume.bounds.center - carry.DeliveryPosition;
                Physics.SyncTransforms();
                await WaitSeconds(0.15f);
                Require(model.State == CatState.Rescued && !input.IsCarrying,
                    "Save zone must rescue a carried cat and free the player's hands.");
                Require(mission.IsActive && !mission.IsWon, "Cat rescue must leave the resident mission running.");
                Require(!carry.TryRescue(carry.DeliveryPosition), "Repeated zone contact must not rescue twice.");
                Require(!carry.GetPrompt(carry.PickupCollider).IsAvailable,
                    "A rescued cat must no longer be pickable.");
                return "PASS: apartment placement, red warning markers, three-second warning, facing the player, " +
                    "retreat cancellation, blocked sight, ballistic hose calming, purr-meow, independent cats, " +
                    "context pickup, carried Save-zone delivery, freed hands, no premature victory or repeat rescue.";
            }
            finally
            {
                input.EndPrimary(PrimaryInputSource.Touch);
                if (blocker != null)
                {
                    Object.Destroy(blocker);
                }
                player.transform.position = originalPosition;
                camera.transform.localRotation = originalRotation;
                character.enabled = true;
                player.enabled = true;
            }
        }

        public static async Task<string> CheckJump()
        {
            var container = FindContainer();
            var camera = container.Resolve<Camera>();
            var input = container.Resolve<InteractionModel>();
            var tools = container.Resolve<ToolSelectionModel>();
            var face = container.Resolve<CatFaceModel>();
            var player = Object.FindAnyObjectByType<FirefighterController>();
            var character = player.GetComponent<CharacterController>();
            var cat = FindCat("cat-upstairs-room");
            var model = cat.GetComponent<GameObjectContext>().Container.Resolve<CatModel>();
            cat.GetComponent<CatPatrolController>().enabled = false;
            cat.GetComponent<GameObjectContext>().Container.Resolve<CatPatrolModel>().SetFireSafety(false, 0f);
            tools.Select(tools.Tools.First(t => t.Action == ToolAction.Hammer).Id);
            player.enabled = false;
            character.enabled = false;
            player.transform.position = new Vector3(-24f, -0.76f, 10f);
            cat.transform.position = new Vector3(-24f, -1.21f, 8f);
            Physics.SyncTransforms();
            camera.transform.LookAt(cat.GetComponent<CatView>().NoticePosition);
            await WaitSeconds(0.2f);
            Require(model.State == CatState.Warning, "The second cat must warn independently.");
            container.Resolve<MissionInputModel>().SetSuspended(true);
            var elapsed = model.StateSeconds;
            await WaitSeconds(0.2f);
            Require(model.StateSeconds == elapsed, "Suspended input must pause the cat attack.");
            container.Resolve<MissionInputModel>().SetSuspended(false);
            var config = cat.GetComponent<GameObjectContext>().Container.Resolve<CatConfigSO>();
            await WaitSeconds(config.WarningSeconds + config.JumpSeconds);
            Require(model.State == CatState.Attached && face.IsAttached,
                "Ignoring the hiss must attach the cat after its jump.");
            Require(input.ContextPrompt.ActionId == HudActionId.RemoveCat,
                "Attachment must advertise removal on the shared context action.");
            EditorApplication.isPaused = true;
            return "PASS: second-cat warning, pause, timed jump, face attachment and removal prompt. " +
                "Ready for an attached-cat capture and CheckRemoval.";
        }

        public static async Task<string> CheckRemoval()
        {
            var container = FindContainer();
            var input = container.Resolve<InteractionModel>();
            var face = container.Resolve<CatFaceModel>();
            var cat = FindCat("cat-upstairs-room");
            var model = cat.GetComponent<GameObjectContext>().Container.Resolve<CatModel>();
            Require(face.IsAttached, "Run CheckJump before checking removal.");
            EditorApplication.isPaused = false;
            input.Interact();
            Require(!face.IsOccupied && model.State == CatState.Returning,
                "The shared interaction action must remove the cat once.");
            await WaitSeconds(0.6f);
            Require(model.State == CatState.Settled, "A removed cat must return and stay calm.");
            Require(cat.GetComponent<CatCarryable>().GetPrompt(cat.GetComponent<CatCarryable>().PickupCollider).IsAvailable,
                "A removed cat must be available for rescue.");
            Object.FindAnyObjectByType<CharacterController>().enabled = true;
            Object.FindAnyObjectByType<FirefighterController>().enabled = true;
            return "PASS: shared context removal, face released, return complete and rescue pickup available.";
        }

        public static async Task<string> CheckResidentRegression()
        {
            var container = FindContainer();
            var resident = container.Resolve<ResidentView>();
            var interaction = container.Resolve<InteractionModel>();
            var mission = container.Resolve<MissionModel>();
            var player = Object.FindAnyObjectByType<FirefighterController>();
            var character = player.GetComponent<CharacterController>();
            var zone = Object.FindAnyObjectByType<RescueZoneView>();
            var volume = zone.GetComponent<BoxCollider>();
            player.enabled = false;
            character.enabled = false;
            interaction.RevealResident();
            resident.GetComponent<Rigidbody>().isKinematic = false;
            resident.Interact();
            Require(interaction.CarriedContentId == resident.ContentId,
                "The shared carrier must still pick up the resident.");
            player.transform.position += volume.bounds.center - resident.DeliveryPosition;
            Physics.SyncTransforms();
            await WaitSeconds(0.2f);
            Require(mission.IsWon && !mission.IsActive && resident.IsRescued,
                "Resident delivery must still finish the mission.");
            Require(!resident.TryRescue(resident.DeliveryPosition), "Resident rescue must remain idempotent.");
            return "PASS: resident pickup, carried Save-zone delivery, victory and duplicate rejection.";
        }

        public static async Task<string> ShowApartmentWarning()
        {
            var container = FindContainer();
            var camera = container.Resolve<Camera>();
            var tools = container.Resolve<ToolSelectionModel>();
            tools.Select(tools.Tools.First(t => t.Action == ToolAction.Hammer).Id);
            var cat = FindCat("cat-ground-hall");
            var player = Object.FindAnyObjectByType<FirefighterController>();
            player.enabled = false;
            player.GetComponent<CharacterController>().enabled = false;
            var target = new Vector3(-12f, 0.05f, -0.85f);
            player.transform.position += target - camera.transform.position;
            camera.transform.LookAt(cat.GetComponent<CatView>().NoticePosition + Vector3.up * 0.2f);
            await WaitSeconds(1.1f);
            Require(cat.GetComponentInChildren<CatWarningView>(true).IsVisible,
                "The cat must show its warning inside the authored apartment.");
            EditorApplication.isPaused = true;
            return "Apartment warning is paused for capture.";
        }

        private static DiContainer FindContainer()
        {
            Require(Application.isPlaying, "Run the integration checks in Play Mode.");
            return Object.FindObjectsByType<SceneContext>().First(c => c.gameObject.scene.name == "Main").Container;
        }

        private static CatController FindCat(string id)
        {
            return Object.FindObjectsByType<CatController>().Single(c => c.ContentId == id);
        }

        private static void AimHose(Camera camera, Vector3 target)
        {
            var hose = Object.FindAnyObjectByType<HoseController>();
            var serialized = new SerializedObject(hose);
            var nozzle = (Transform)serialized.FindProperty("_nozzle").objectReferenceValue;
            var config = (HoseConfigSO)serialized.FindProperty("_config").objectReferenceValue;
            for (var iteration = 0; iteration < 8; iteration++)
            {
                var flightSeconds = Vector3.Distance(nozzle.position, target) / config.LaunchSpeed;
                var gravityOffset = Physics.gravity * (config.GravityScale * flightSeconds * flightSeconds * 0.5f);
                camera.transform.rotation = Quaternion.LookRotation(target - nozzle.position - gravityOffset);
            }
        }

        private static void RefreshContext(InteractionController controller)
        {
            var method = typeof(InteractionController).GetMethod("ResolveTarget", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(controller, null);
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
