using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FireSlop.Cats;
using FireSlop.Firefighter;
using FireSlop.Interaction;
using FireSlop.Mission;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Zenject;
using Object = UnityEngine.Object;

namespace FireSlop.Verification
{
    public static class CatDynamicNavigationVerification
    {
        public static async Task<string> CheckFurniture()
        {
            var context = FindContainer();
            var input = context.Resolve<MissionInputModel>();
            var cat = Object.FindObjectsByType<CatController>().Single(c => c.ContentId == "cat-ground-hall");
            var area = Object.FindObjectsByType<CatPatrolArea>().Single(a => a.name.Contains("Ground"));
            var furniture = Object.FindObjectsByType<Movable>().First(m => m.name.Contains("Sofa") &&
                area.Contains(m.transform.position));
            var obstacle = furniture.GetComponent<NavMeshObstacle>();
            Require(obstacle != null && obstacle.carving && obstacle.carveOnlyStationary,
                "Movable furniture needs stationary carving.");
            var body = furniture.GetComponent<Rigidbody>();
            var original = body.position;
            var wasKinematic = body.isKinematic;
            var filter = new NavMeshQueryFilter
            {
                agentTypeID = cat.GetComponent<NavMeshAgent>().agentTypeID,
                areaMask = NavMesh.AllAreas
            };
            var footprint = obstacle.transform.TransformPoint(obstacle.center);
            footprint.y = cat.transform.position.y;
            input.SetSuspended(true);
            body.isKinematic = true;
            try
            {
                body.position = original + Vector3.right * 8f;
                Physics.SyncTransforms();
                await WaitSeconds(0.7f);
                Require(NavMesh.SamplePosition(footprint, out _, 0.08f, filter),
                    "Moving the sofa away must reopen its old footprint; no baked ghost hole is allowed.");
                var target = new Vector3(-12f, cat.transform.position.y, -1.7f);
                var centerOffset = Vector3.ProjectOnPlane(obstacle.transform.TransformPoint(obstacle.center) - body.position,
                    Vector3.up);
                var lineStart = target + Vector3.forward * 1.2f;
                var lineEnd = target - Vector3.forward * 1.2f;
                var patrol = cat.GetComponent<CatPatrolController>();
                var method = typeof(CatPatrolController).GetMethod("IsMovementClear",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Require((bool)method.Invoke(patrol, new object[] { lineStart, lineEnd }),
                    "The test corridor must be clear before moving furniture into it.");
                body.position = new Vector3(target.x - centerOffset.x, original.y, target.z - centerOffset.z);
                Physics.SyncTransforms();
                Require(!(bool)method.Invoke(patrol, new object[] { lineStart, lineEnd }),
                    "A moving sofa must block cat movement immediately, before carving settles.");
                await WaitSeconds(0.7f);
                Require(!NavMesh.SamplePosition(target, out _, 0.08f, filter),
                    "The stopped sofa must carve its new footprint.");
                body.position = original + Vector3.right * 8f;
                Physics.SyncTransforms();
                await WaitSeconds(0.7f);
                Require(NavMesh.SamplePosition(target, out _, 0.08f, filter),
                    "Moving the sofa again must reopen the temporary footprint.");
                return "PASS: old furniture footprint reopened, immediate collision guard, new carving and reopened route.";
            }
            finally
            {
                body.position = original;
                body.isKinematic = wasKinematic;
                Physics.SyncTransforms();
                input.SetSuspended(false);
                await WaitSeconds(0.7f);
            }
        }

        public static async Task<string> CheckWaterPush()
        {
            FindContainer();
            var area = Object.FindObjectsByType<CatPatrolArea>().Single(a => a.name.Contains("Ground"));
            var furniture = Object.FindObjectsByType<Movable>().First(m => m.name.Contains("Sofa") &&
                area.Contains(m.transform.position));
            var body = furniture.GetComponent<Rigidbody>();
            Require(!body.isKinematic, "Navigation must not make movable furniture kinematic.");
            var position = body.position;
            var rotation = body.rotation;
            try
            {
                var collider = furniture.GetComponentsInChildren<Collider>().First(c => !c.isTrigger &&
                    c.attachedRigidbody == body);
                var ray = new Ray(collider.bounds.center + Vector3.up * 2f, Vector3.down);
                Require(collider.Raycast(ray, out var hit, 4f), "Hit the sofa's actual physics body.");
                var water = new WaterImpact(hit, Vector3.forward, Time.fixedDeltaTime, body.mass * 3f);
                furniture.ApplyWater(water);
                await WaitSeconds(0.2f);
                Require(Vector3.Distance(position, body.position) > 0.01f,
                    "Water must still push furniture with a navigation obstacle attached.");
                return "PASS: water impulse still moves the sofa; navigation leaves furniture physics authoritative.";
            }
            finally
            {
                body.position = position;
                body.rotation = rotation;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                Physics.SyncTransforms();
                await WaitSeconds(0.7f);
            }
        }

        public static async Task<string> CheckCooldown()
        {
            var container = FindContainer();
            var camera = container.Resolve<Camera>();
            var input = container.Resolve<InteractionModel>();
            var cat = Object.FindObjectsByType<CatController>().Single(c => c.ContentId == "cat-ground-hall");
            var carry = cat.GetComponent<CatCarryable>();
            cat.GetComponent<CatPatrolController>().enabled = false;
            var local = cat.GetComponent<GameObjectContext>().Container;
            var model = local.Resolve<CatModel>();
            var config = local.Resolve<CatConfigSO>();
            local.Resolve<CatPatrolModel>().SetFireSafety(false, 0f);
            var player = Object.FindAnyObjectByType<FirefighterController>();
            var character = player.GetComponent<CharacterController>();
            var originalPosition = player.transform.position;
            var oldRotation = camera.transform.localRotation;
            player.enabled = false;
            character.enabled = false;
            try
            {
                var eyePosition = cat.transform.position + new Vector3(0f, 1f, 1.4f);
                player.transform.position += eyePosition - camera.transform.position;
                camera.transform.LookAt(cat.GetComponent<CatView>().NoticePosition);
                Require(model.ApplyWater(config.WaterSeconds, config.WaterSeconds), "Start with a calm free cat.");
                await WaitSeconds(0.2f);
                Require(model.State == CatState.Settled, "Cooldown must prevent an immediate attack.");
                carry.Interact();
                Require(carry.IsCarried, "The cooling-down cat must be pickable.");
                var elapsed = model.StateSeconds;
                await WaitSeconds(1f);
                Require(model.State == CatState.Settled && Math.Abs(model.StateSeconds - elapsed) < 0.01f,
                    "The cooldown clock must pause while the cat is carried.");
                camera.transform.rotation = Quaternion.LookRotation(Vector3.back);
                var interaction = Object.FindAnyObjectByType<InteractionController>();
                typeof(InteractionController).GetMethod("ResolveTarget", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(interaction, null);
                input.Interact();
                Require(!carry.IsCarried, "The shared context action must put down the cat.");
                await WaitSeconds(config.AttackCooldownSeconds - model.StateSeconds + 0.2f);
                Require(model.State == CatState.Warning, "After the authored cooldown the nearby cat must warn again.");
                Require(cat.GetComponent<Rigidbody>().isKinematic,
                    "A rearmed cat must not fall through the floor when its colliders become triggers.");
                var takeoff = cat.transform.position;
                await WaitSeconds(config.WarningSeconds + config.AttackFlightSeconds + 0.1f);
                Require(model.State == CatState.Attached, "The repeated encounter must actually jump and attach.");
                Require(Vector3.Distance(takeoff, cat.transform.position) < 0.01f,
                    "A jumping cat must preserve its physical takeoff position.");
                for (var index = 0; index < config.RemovalPresses; index++)
                {
                    input.Interact();
                }
                await WaitSeconds(config.JumpSeconds + 0.1f);
                Require(model.State == CatState.Settled, "Removal must start another rescue/cooldown window.");
                carry.Interact();
                var zone = Object.FindAnyObjectByType<RescueZoneView>().GetComponent<BoxCollider>();
                player.transform.position += zone.bounds.center - carry.DeliveryPosition;
                Physics.SyncTransforms();
                await WaitSeconds(0.2f);
                Require(model.State == CatState.Rescued, "A repeated attacker can still be rescued.");
                model.Advance(config.AttackCooldownSeconds * 2f);
                await WaitSeconds(0.1f);
                Require(model.State == CatState.Rescued, "A rescued cat must stay safe beyond cooldown expiry.");
                return "PASS: SO cooldown, carried pause, put-down, renewed warning, repeat jump, removal and final rescue.";
            }
            finally
            {
                player.transform.position = originalPosition;
                camera.transform.localRotation = oldRotation;
                character.enabled = true;
                player.enabled = true;
            }
        }

        private static DiContainer FindContainer()
        {
            Require(Application.isPlaying, "Start a fresh Play Mode run.");
            return Object.FindObjectsByType<SceneContext>().Single(c => c.gameObject.scene.name == "Main").Container;
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
