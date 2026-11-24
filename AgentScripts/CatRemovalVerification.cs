using System;
using System.Linq;
using System.Threading.Tasks;
using FireSlop.Cats;
using FireSlop.Firefighter;
using FireSlop.Interaction;
using FireSlop.MissionHud;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Object = UnityEngine.Object;

namespace FireSlop.Verification
{
    public static class CatRemovalVerification
    {
        public static async Task<string> Run()
        {
            var context = Object.FindObjectsByType<SceneContext>().Single(item => item.gameObject.scene.name == "Main");
            var input = context.Container.Resolve<InteractionModel>();
            var face = context.Container.Resolve<CatFaceModel>();
            var camera = context.Container.Resolve<Camera>();
            var player = Object.FindAnyObjectByType<FirefighterController>();
            var character = player.GetComponent<CharacterController>();
            var cat = Object.FindObjectsByType<CatController>().Single(item => item.ContentId == "cat-ground-hall");
            var other = Object.FindObjectsByType<CatController>().Single(item => item.ContentId == "cat-upstairs-room");
            var model = cat.GetComponent<GameObjectContext>().Container.Resolve<CatModel>();
            var otherModel = other.GetComponent<GameObjectContext>().Container.Resolve<CatModel>();
            var config = cat.GetComponent<GameObjectContext>().Container.Resolve<CatConfigSO>();
            var view = cat.GetComponent<CatView>();
            var visual = (Transform)new SerializedObject(view).FindProperty("_visual").objectReferenceValue;
            var body = cat.GetComponent<Rigidbody>();
            player.enabled = false;
            character.enabled = false;
            cat.GetComponent<CatPatrolController>().enabled = false;
            cat.GetComponent<GameObjectContext>().Container.Resolve<CatPatrolModel>().SetFireSafety(false, 0f);
            try
            {
                Require(config.FaceTurnSpeed == 720f && config.RoamTurnSpeed == 180f,
                    "Facing must be four times faster without changing patrol turns.");
                cat.transform.rotation = Quaternion.identity;
                view.FacePlayer(cat.transform.position + Vector3.right, config.FaceTurnSpeed, 0.125f);
                Require(Vector3.Dot(cat.transform.forward, Vector3.right) > 0.999f,
                    "Facing must complete a 90-degree turn in 0.125 seconds.");
                player.transform.position = new Vector3(-24f, -0.76f, 10f);
                camera.transform.rotation = Quaternion.LookRotation(Vector3.back);
                cat.transform.position = new Vector3(-24f, -1.21f, 8f);
                Physics.SyncTransforms();
                model.SetThreatened(true);
                model.Advance(config.WarningSeconds);
                Require(face.TryReserve(cat.ContentId) && model.TryJump(config.WarningSeconds),
                    "The fixture must start a valid face attack.");
                await Wait(config.JumpSeconds + 0.1f);
                Require(model.State == CatState.Attached && face.IsAttached, "The cat must attach normally.");
                var takeoffPosition = cat.transform.position;
                player.transform.position = new Vector3(-44f, -0.76f, 10f);
                camera.transform.rotation = Quaternion.Euler(18f, 180f, 0f);
                Physics.SyncTransforms();
                await Wait(0.08f);
                var attachedPosition = visual.position;
                var attachedRotation = visual.rotation;
                Require(Vector3.Distance(attachedPosition, takeoffPosition) > 15f,
                    "The player must have moved well away from the takeoff point.");
                Require(input.ContextPrompt.ActionId == HudActionId.RemoveCat, "E must advertise cat removal.");
                var meter = Object.FindAnyObjectByType<CatRemovalMeterView>();
                var fillImage = (Image)new SerializedObject(meter).FindProperty("_fill").objectReferenceValue;
                input.Interact();
                Require(face.IsAttached && fillImage.isActiveAndEnabled && fillImage.fillAmount == face.Progress,
                    "The first press must show partial progress.");
                await Wait(config.RemovalGapSeconds + 0.2f);
                Require(face.IsAttached && face.Progress == 0f && fillImage.fillAmount == 0f,
                    "A pause must empty the visible meter.");
                attachedPosition = visual.position;
                attachedRotation = visual.rotation;
                for (var index = 0; index < config.RemovalPresses; index++)
                {
                    input.Interact();
                }
                Require(model.State == CatState.Returning && !face.IsOccupied,
                    "Completing the series must detach the cat.");
                Require(Vector3.Distance(visual.position, attachedPosition) < 0.001f &&
                    Quaternion.Angle(visual.rotation, attachedRotation) < 0.01f,
                    "Detaching must preserve the visible pose without teleporting.");
                Require(!body.isKinematic && cat.GetComponents<Collider>().All(item => item.enabled && !item.isTrigger),
                    "The physical cat must fall with solid colliders at its visible position.");
                var releasedPosition = body.position;
                input.Interact();
                Require(Vector3.Distance(body.position, releasedPosition) < 0.001f,
                    "Repeating E must not rebase or remove the same cat again.");
                await Wait(0.15f);
                Require(body.position.y < releasedPosition.y - 0.01f,
                    "Gravity must move the detached cat down from the face.");
                await Wait(config.JumpSeconds + 0.1f);
                Require(model.State == CatState.Settled && Vector3.Distance(body.position, takeoffPosition) > 15f,
                    "Completing the removal must never return to the old takeoff position.");
                Require(Vector3.Distance(body.position, player.transform.position) < 3f,
                    "The removed cat must remain near the player's current position.");
                var carryable = cat.GetComponent<CatCarryable>();
                Require(carryable.GetPrompt(carryable.PickupCollider).IsAvailable,
                    "The detached cat must remain available for rescue pickup.");
                carryable.Interact();
                Require(carryable.IsCarried, "A removed cat must still use the normal pickup operation.");
                Require(Vector3.Dot(carryable.transform.forward, -camera.transform.forward) > 0.99f,
                    "The carried cat must face its carrier.");
                Require(carryable.TryRescue(cat.transform.position + Vector3.forward * 2f),
                    "A removed carried cat must remain rescuable.");
                Require(model.State == CatState.Rescued && !carryable.IsCarried,
                    "Rescue must finish without restoring the old position or carry state.");
                Require(otherModel.State == CatState.Idle, "Removing one cat must not change the other cat.");
                return "PASS: shared E removal after 20 m of travel preserves visible position/rotation, enables " +
                    "gravity and collisions, stays near the player, rejects repeat removal and supports pickup/rescue; " +
                    "facing is 720 deg/s with patrol unchanged at 180 deg/s.";
            }
            finally
            {
                character.enabled = true;
                player.enabled = true;
            }
        }

        private static async Task Wait(float seconds)
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
