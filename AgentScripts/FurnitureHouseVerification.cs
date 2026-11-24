using System;
using System.Collections.Generic;
using System.IO;
using FireSlop.Interaction;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FireSlop.Verification
{
    public static class FurnitureHouseVerification
    {
        private const string Report = "Documentation/Art/ModelProduction/furniture-house-authoring.json";

        public static string CheckSaved()
        {
            Require(!EditorApplication.isPlaying, "Saved-scene checks require Edit Mode.");
            var scene = SceneManager.GetSceneByPath("Assets/Scenes/Main.unity");
            var opened = !scene.isLoaded;
            if (opened)
            {
                scene = EditorSceneManager.OpenScene("Assets/Scenes/Main.unity", OpenSceneMode.Additive);
            }
            try
            {
                var added = new List<Movable>();
                var allColliders = new List<Collider>();
                var ids = new HashSet<string>();
                foreach (var root in scene.GetRootGameObjects())
                {
                    allColliders.AddRange(root.GetComponentsInChildren<Collider>(true));
                    foreach (var movable in root.GetComponentsInChildren<Movable>(true))
                    {
                        Require(ids.Add(movable.ContentId) && !string.IsNullOrWhiteSpace(movable.ContentId),
                            "Every furniture instance requires a unique authored ID.");
                        if (movable.ContentId.StartsWith("furniture-floor-"))
                        {
                            added.Add(movable);
                        }
                    }
                }

                Require(added.Count == 21, "Expected 21 new furniture instances.");
                var rows = new JArray();
                var overlaps = new JArray();
                Physics.SyncTransforms();
                foreach (var movable in added)
                {
                    Require(PrefabUtility.IsAnyPrefabInstanceRoot(movable.gameObject) &&
                        PrefabUtility.GetPrefabInstanceStatus(movable) == PrefabInstanceStatus.Connected,
                        "Furniture must remain a connected prefab instance.");
                    var doors = movable.GetComponentsInChildren<Openable>(true);
                    foreach (var door in doors)
                    {
                        Require(door.GetComponent<CabinetContextTarget>() != null, "Every door needs a context receiver.");
                    }

                    rows.Add(new JObject { ["id"] = movable.ContentId, ["prefab"] =
                        PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(movable),
                        ["position"] = Vector(movable.transform.position),
                        ["rotation"] = Vector(movable.transform.eulerAngles),
                        ["doors"] = doors.Length, ["connected"] = true });
                    foreach (var collider in movable.GetComponentsInChildren<Collider>())
                    {
                        foreach (var other in allColliders)
                        {
                            if (!other.enabled || other.isTrigger || !other.gameObject.activeInHierarchy ||
                                other.attachedRigidbody == collider.attachedRigidbody || !collider.bounds.Intersects(other.bounds))
                            {
                                continue;
                            }

                            if (Physics.ComputePenetration(collider, collider.transform.position, collider.transform.rotation,
                                other, other.transform.position, other.transform.rotation, out var direction, out var distance) &&
                                distance > 0.025f)
                            {
                                overlaps.Add(new JObject { ["id"] = movable.ContentId, ["part"] = collider.name,
                                    ["other"] = PathOf(other.transform), ["depth"] = distance });
                            }
                        }
                    }
                }

                var data = new JObject { ["furniture"] = rows, ["overlaps"] = overlaps };
                File.WriteAllText(Report, data.ToString() + "\n");
                return "Verified " + added.Count + " connected instances; overlap candidates: " + overlaps.Count;
            }
            finally
            {
                if (opened)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static JArray Vector(Vector3 value)
        {
            return new JArray(value.x, value.y, value.z);
        }

        private static string PathOf(Transform transform)
        {
            return transform.parent == null ? transform.name : PathOf(transform.parent) + "/" + transform.name;
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
