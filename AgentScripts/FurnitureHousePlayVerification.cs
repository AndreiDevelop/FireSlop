using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using FireSlop.Cats;
using FireSlop.Firefighter;
using FireSlop.Interaction;
using FireSlop.Mission;
using FireSlop.Tools;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using Object = UnityEngine.Object;

namespace FireSlop.Verification
{
    public static class FurnitureHousePlayVerification
    {
        private const string ArtRoot = "Documentation/Art/ModelProduction/";
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        public static async Task<string> CheckOpeningAndPaths()
        {
            var context = Container();
            var camera = context.Resolve<Camera>();
            var interaction = context.Resolve<InteractionModel>();
            var controller = Object.FindAnyObjectByType<InteractionController>();
            var player = Object.FindAnyObjectByType<FirefighterController>();
            var character = player.GetComponent<CharacterController>();
            var previousPosition = camera.transform.position;
            var previousRotation = camera.transform.rotation;
            player.enabled = false;
            character.enabled = false;
            foreach (var cat in Object.FindObjectsByType<CatController>())
            {
                cat.enabled = false;
            }

            foreach (var spread in Object.FindObjectsByType<FireSpreadController>())
            {
                spread.enabled = false;
            }

            foreach (var fire in Object.FindObjectsByType<WaterFireTarget>())
            {
                var water = new WaterImpact(default, Vector3.forward, 10f, 0f);
                fire.ApplyWater(water);
            }

            await WaitSeconds(0.2f);
            var resolve = typeof(InteractionController).GetMethod("ResolveTarget", PrivateInstance);
            var furniture = NewFurniture();
            var results = new JArray();
            foreach (var movable in furniture)
            {
                foreach (var door in movable.GetComponentsInChildren<Openable>())
                {
                    if (door.IsOpen)
                    {
                        door.TryToggle();
                    }
                }
            }

            await WaitSeconds(0.7f);
            foreach (var movable in furniture)
            {
                foreach (var door in movable.GetComponentsInChildren<Openable>())
                {
                    Require(door.CanToggle && !door.IsOpen, "A new run must start with unlocked, closed furniture panels.");
                    var collider = door.GetComponentInChildren<BoxCollider>();
                    var hinge = door.transform;
                    var closed = hinge.localRotation;
                    Aim(camera, collider, hinge.forward);
                    Physics.SyncTransforms();
                    resolve.Invoke(controller, null);
                    Require(interaction.ContextPrompt.ActionId == HudActionId.OpenCabinet,
                        "Aiming at " + movable.ContentId + "/" + door.name + " must advertise Open.");
                    interaction.Interact();
                    await WaitSeconds(0.6f);
                    Require(door.IsFullyOpen && Quaternion.Angle(closed, hinge.localRotation) > 30f,
                        "The shared context input must animate the selected panel.");
                    foreach (var other in movable.GetComponentsInChildren<Openable>())
                    {
                        Require(other == door || !other.IsOpen, "Opening a panel must preserve its sibling's state.");
                    }

                    Aim(camera, collider, hinge.forward);
                    Physics.SyncTransforms();
                    resolve.Invoke(controller, null);
                    Require(interaction.ContextPrompt.ActionId == HudActionId.CloseCabinet,
                        "An open panel must advertise Close: " + movable.ContentId + "/" + door.name);
                    interaction.Interact();
                    await WaitSeconds(0.6f);
                    Require(!door.IsOpen && Quaternion.Angle(closed, hinge.localRotation) < 0.2f,
                        "The shared context input must restore the authored closed pose.");
                    results.Add(new JObject { ["id"] = movable.ContentId, ["panel"] = door.name,
                        ["contextOpenClose"] = true, ["independentState"] = true });
                }
            }

            Require(results.Count == 21, "Expected 21 independent opening panels in Main.");
            var closedRoutes = CheckRoutes();
            foreach (var movable in furniture)
            {
                foreach (var door in movable.GetComponentsInChildren<Openable>())
                {
                    door.GetComponent<CabinetContextTarget>().Interact();
                }
            }

            await WaitSeconds(0.7f);
            var openRoutes = CheckRoutes();
            File.WriteAllText(ArtRoot + "furniture-opening-validation.json", new JObject
            {
                ["panels"] = results, ["closedRoutes"] = closedRoutes, ["openRoutes"] = openRoutes
            }.ToString() + "\n");
            camera.transform.SetPositionAndRotation(previousPosition, previousRotation);
            player.enabled = true;
            character.enabled = true;
            return "21 panels passed shared-context Open/Close and sibling isolation; routes checked closed and open.";
        }

        public static async Task<string> CheckPushAndRestart()
        {
            var context = Container();
            var furniture = NewFurniture();
            var results = new JArray();
            var tested = new HashSet<string>();
            foreach (var movable in furniture)
            {
                if (!tested.Add(movable.name))
                {
                    continue;
                }

                var body = movable.GetComponent<Rigidbody>();
                var original = body.position;
                var collider = movable.GetComponentInChildren<BoxCollider>();
                var ray = new Ray(collider.bounds.center + Vector3.up * 2f, Vector3.down);
                Require(collider.Raycast(ray, out var hit, 4f), "Water must hit authored furniture collision.");
                var direction = movable.transform.forward;
                var water = new WaterImpact(hit, direction, Time.fixedDeltaTime, body.mass * 3f);
                hit.collider.GetComponentInParent<IWaterTarget>().ApplyWater(water);
                await WaitSeconds(0.3f);
                var moved = Vector3.Distance(original, body.position);
                Require(moved > 0.01f, movable.name + " must still move under water with open panels.");
                results.Add(new JObject { ["id"] = movable.ContentId, ["movedMeters"] = moved, ["passed"] = true });
            }

            Require(results.Count == 5, "Water response must cover all five furniture types.");
            File.WriteAllText(ArtRoot + "furniture-house-water-validation.json", results.ToString() + "\n");
            var first = furniture[0];
            var oldInstance = first.GetEntityId();
            var id = first.ContentId;
            var oldMission = context.Resolve<MissionModel>();
            // Set only the result gate; avoid granting ranking credit during verification.
            typeof(MissionModel).GetField("<IsWon>k__BackingField", PrivateInstance).SetValue(oldMission, true);
            context.Resolve<MissionController>().Restart();
            var deadline = Time.realtimeSinceStartup + 15f;
            while (Time.realtimeSinceStartup < deadline)
            {
                var candidates = NewFurniture();
                if (candidates.Count == 21 && candidates.Exists(item => item.ContentId == id && item.GetEntityId() != oldInstance))
                {
                    await WaitSeconds(0.3f);
                    return CheckRestored();
                }

                await Task.Delay(30);
            }

            throw new InvalidOperationException("Restart did not reload the authored furniture in time.");
        }

        public static string CheckRestored()
        {
            var expected = JObject.Parse(File.ReadAllText(ArtRoot + "furniture-house-authoring.json"))["furniture"];
            var furniture = NewFurniture();
            Require(furniture.Count == 21, "Restart must restore exactly 21 new furniture instances without duplicates.");
            var results = new JArray();
            foreach (var row in expected)
            {
                var id = row["id"].Value<string>();
                var movable = furniture.Find(item => item.ContentId == id);
                Require(movable != null, "Restart lost " + id);
                var position = row["position"];
                var authored = new Vector3(position[0].Value<float>(), position[1].Value<float>(), position[2].Value<float>());
                var euler = row["rotation"];
                var rotation = Quaternion.Euler(euler[0].Value<float>(), euler[1].Value<float>(), euler[2].Value<float>());
                var distance = Vector3.Distance(movable.transform.position, authored);
                Require(distance < 0.04f && Quaternion.Angle(movable.transform.rotation, rotation) < 1f,
                    "Restart must restore the saved pose of " + id + ": error=" + distance);
                foreach (var door in movable.GetComponentsInChildren<Openable>())
                {
                    Require(!door.IsOpen && door.CanToggle && Quaternion.Angle(door.transform.localRotation, Quaternion.identity) < 0.2f,
                        "Restart must restore independent closed/unlocked panels.");
                }

                results.Add(new JObject { ["id"] = id, ["positionErrorMeters"] = distance,
                    ["closed"] = true, ["passed"] = true });
            }

            File.WriteAllText(ArtRoot + "furniture-restart-validation.json", results.ToString() + "\n");
            return "Actual MissionController.Restart restored all 21 poses and all 21 panels with no duplicate furniture.";
        }

        private static JArray CheckRoutes()
        {
            var routes = new JArray();
            for (var floor = 1; floor <= 3; floor++)
            {
                var y = -1.21f + (floor - 1) * 3f;
                CheckSegment(new Vector3(-14.6f, y, 0f), new Vector3(-14.6f, y, 4.6f), floor, "east-entry", routes);
                CheckSegment(new Vector3(-11.9f, y, -1f), new Vector3(-11.9f, y, -2.8f), floor, "kitchen", routes);
                CheckSegment(new Vector3(-17.8f, y, 0.6f), new Vector3(-18f, y, 1.7f), floor, "west-entry-a", routes);
                CheckSegment(new Vector3(-18f, y, 1.7f), new Vector3(-18.3f, y, 2.8f), floor, "west-entry-b", routes);
            }

            return routes;
        }

        private static void CheckSegment(Vector3 from, Vector3 to, int floor, string name, JArray results)
        {
            Physics.SyncTransforms();
            var delta = to - from;
            var hits = Physics.CapsuleCastAll(from + Vector3.up * 0.4f, from + Vector3.up * 1.45f, 0.34f,
                delta.normalized, delta.magnitude, ~0, QueryTriggerInteraction.Ignore);
            foreach (var hit in hits)
            {
                var movable = hit.collider.GetComponentInParent<Movable>();
                var obstructionId = movable != null ? movable.ContentId : string.Empty;
                Require(movable == null || !movable.ContentId.StartsWith("furniture-floor-"),
                    "New furniture blocks floor " + floor + " " + name + ": " + obstructionId);
            }

            results.Add(new JObject { ["floor"] = floor, ["route"] = name,
                ["capsuleRadiusMeters"] = 0.34f, ["newFurnitureClear"] = true });
        }

        private static void Aim(Camera camera, Collider collider, Vector3 normal)
        {
            var target = collider.bounds.center;
            var horizontal = Vector3.ProjectOnPlane(normal, Vector3.up).normalized;
            var side = Vector3.Cross(horizontal, Vector3.up);
            var directions = new[] { horizontal, -horizontal, side, -side,
                (horizontal + side).normalized, (horizontal - side).normalized,
                (-horizontal + side).normalized, (-horizontal - side).normalized };
            foreach (var direction in directions)
            {
                var position = target + direction * 0.8f + Vector3.up * 0.6f;
                var delta = target - position;
                if (Physics.CheckSphere(position, 0.1f, ~0, QueryTriggerInteraction.Ignore))
                {
                    continue;
                }

                if (Physics.Raycast(position, delta.normalized, out var hit, delta.magnitude + 0.05f,
                    ~0, QueryTriggerInteraction.Ignore) && hit.collider == collider)
                {
                    camera.transform.position = position;
                    camera.transform.LookAt(target);
                    return;
                }
            }

            throw new InvalidOperationException("No unobstructed interaction viewpoint for " + collider.name);
        }

        private static List<Movable> NewFurniture()
        {
            var result = new List<Movable>();
            foreach (var movable in Object.FindObjectsByType<Movable>())
            {
                if (movable.ContentId != null && movable.ContentId.StartsWith("furniture-floor-"))
                {
                    result.Add(movable);
                }
            }

            return result;
        }

        private static DiContainer Container()
        {
            foreach (var context in Object.FindObjectsByType<SceneContext>())
            {
                if (context.Container.HasBinding<MissionModel>())
                {
                    return context.Container;
                }
            }

            throw new InvalidOperationException("Start Main in Play Mode before checking furniture behavior.");
        }

        private static async Task WaitSeconds(float seconds)
        {
            var end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end)
            {
                await Task.Delay(10);
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
