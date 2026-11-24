using System;
using System.Collections.Generic;
using System.IO;
using FireSlop.Interaction;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace FireSlop.Verification
{
    public static class FurnitureVerification
    {
        private const string ArtRoot = "Documentation/Art/ModelProduction/";
        private const string PrefabRoot = "Assets/Prefabs/Props/Furniture/";
        private const string ReviewPath = "Assets/Scenes/FurnitureReview.unity";
        private static readonly string[] Names = { "Fridge", "Stove", "DiningChair", "CoatRack", "ShoeCabinet" };

        public static string CheckAssets()
        {
            var records = JArray.Parse(File.ReadAllText(ArtRoot + "furniture-blender-validation.json"));
            var results = new JArray();
            for (var index = 0; index < Names.Length; index++)
            {
                var name = Names[index];
                var path = PrefabRoot + name + ".prefab";
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    Require(root.transform.localPosition == Vector3.zero && root.transform.localRotation == Quaternion.identity &&
                        root.transform.localScale == Vector3.one, name + " root must have identity transform.");
                    Require(root.layer == 10, name + " must use the movable furniture layer.");
                    var visual = root.transform.Find("Visual");
                    Require(visual != null && PrefabUtility.IsPartOfModelPrefab(visual), name + " needs a connected FBX visual.");
                    Require(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(visual) ==
                        "Assets/Models/Props/" + name + ".fbx", name + " FBX link mismatch.");
                    var meshes = root.GetComponentsInChildren<MeshFilter>(true);
                    Require(meshes.Length > 0 && meshes[0].sharedMesh != null, name + " needs valid authored meshes.");
                    var renderers = root.GetComponentsInChildren<MeshRenderer>(true);
                    Require(renderers.Length == meshes.Length, name + " renderer/mesh counts must match.");
                    var bounds = renderers[0].bounds;
                    foreach (var renderer in renderers)
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }
                    var dimensions = records[index]["dimensionsBlender"];
                    var expected = new Vector3(dimensions[0].Value<float>(), dimensions[2].Value<float>(), dimensions[1].Value<float>());
                    Require(Vector3.Distance(bounds.size, expected) < 0.004f && Mathf.Abs(bounds.min.y) < 0.002f,
                        name + " scale or ground pivot mismatch.");
                    var sharedMaterials = new List<Material>();
                    foreach (var renderer in renderers)
                    {
                        sharedMaterials.AddRange(renderer.sharedMaterials);
                    }

                    foreach (var material in sharedMaterials)
                    {
                        Require(material != null && AssetDatabase.GetAssetPath(material).StartsWith("Assets/Materials/Models/"),
                            name + " needs external shared materials.");
                        Require(material.shader.name == "Universal Render Pipeline/Lit", name + " material must use URP Lit.");
                    }

                    foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                    {
                        Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject) == 0,
                            name + " has a missing script.");
                    }

                    var body = root.GetComponent<Rigidbody>();
                    var movable = root.GetComponent<Movable>();
                    var data = new SerializedObject(movable);
                    var settings = data.FindProperty("_settings").objectReferenceValue as FurniturePhysicsSO;
                    Require(body != null && !body.isKinematic && settings != null && settings.Mass == body.mass,
                        name + " physics configuration mismatch.");
                    Require(!string.IsNullOrEmpty(data.FindProperty("_contentId").stringValue), name + " requires a content ID.");
                    var navigation = root.GetComponent<FurnitureNavigationView>();
                    Require(navigation != null && new SerializedObject(navigation).FindProperty("_config").objectReferenceValue != null,
                        name + " navigation configuration is missing.");
                    var obstacle = root.GetComponent<NavMeshObstacle>();
                    Require(obstacle != null && obstacle.carving && obstacle.carveOnlyStationary,
                        name + " requires stationary obstacle carving.");
                    var colliders = root.GetComponentsInChildren<Collider>(true);
                    Require(colliders.Length > 0, name + " needs primitive collision.");
                    foreach (var collider in colliders)
                    {
                        Require(collider is BoxCollider && collider.enabled && !collider.isTrigger &&
                            collider.attachedRigidbody == body && collider.sharedMaterial != null,
                            name + " collider setup mismatch.");
                    }

                    uint triangles = 0;
                    var submeshes = 0;
                    foreach (var filter in meshes)
                    {
                        var mesh = filter.sharedMesh;
                        submeshes += mesh.subMeshCount;
                        for (var submesh = 0; submesh < mesh.subMeshCount; submesh++)
                        {
                            triangles += mesh.GetIndexCount(submesh) / 3;
                        }
                    }

                    Require(triangles == records[index]["triangles"].Value<uint>(), name + " triangle count changed on import.");
                    results.Add(new JObject
                    {
                        ["name"] = name,
                        ["prefab"] = path,
                        ["dimensionsMetersXYZ"] = new JArray(bounds.size.x, bounds.size.y, bounds.size.z),
                        ["triangles"] = triangles,
                        ["submeshes"] = submeshes,
                        ["renderers"] = renderers.Length,
                        ["boxColliders"] = colliders.Length,
                        ["massKg"] = settings.Mass,
                        ["connectedFBX"] = true,
                        ["passed"] = true
                    });
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            File.WriteAllText(ArtRoot + "furniture-unity-validation.json", results.ToString() + "\n");
            return "Five saved prefabs passed scale, ground pivot, FBX connectivity, material and component checks.";
        }

        public static string CaptureReview()
        {
            var scene = EditorSceneManager.OpenScene(ReviewPath, OpenSceneMode.Additive);
            try
            {
                foreach (var root in scene.GetRootGameObjects())
                {
                    var camera = root.GetComponent<Camera>();
                    if (camera != null)
                    {
                        Capture(camera, ArtRoot + "furniture-unity-preview.png");
                        return "Captured the authored FurnitureReview scene.";
                    }
                }

                throw new InvalidOperationException("Review scene requires an authored camera.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        public static string CheckReview()
        {
            var scene = EditorSceneManager.OpenScene(ReviewPath, OpenSceneMode.Additive);
            try
            {
                var count = 0;
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (root.GetComponent<Movable>() != null)
                    {
                        Require(PrefabUtility.IsAnyPrefabInstanceRoot(root) &&
                            PrefabUtility.GetPrefabInstanceStatus(root) == PrefabInstanceStatus.Connected,
                            "Review furniture must remain connected after reopening.");
                        Require(root.activeSelf && root.transform.position.y == 0f, "Review furniture must be visible on the ground.");
                        count++;
                    }
                }

                Require(count == 6, "Review scene must contain five new models and one existing table, with no duplicates.");
                return "Reopened review scene: all six furniture instances remain connected and visible.";
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void Capture(Camera camera, string path)
        {
            var renderTexture = new RenderTexture(1600, 720, 24);
            var image = new Texture2D(1600, 720, TextureFormat.RGB24, false);
            var previous = RenderTexture.active;
            try
            {
                camera.targetTexture = renderTexture;
                camera.Render();
                RenderTexture.active = renderTexture;
                image.ReadPixels(new Rect(0, 0, 1600, 720), 0, 0);
                image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                Object.DestroyImmediate(image);
                Object.DestroyImmediate(renderTexture);
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
