using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FireSlop.Cats;
using FireSlop.Firefighter;
using FireSlop.Interaction;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Zenject;
using Object = UnityEngine.Object;

namespace FireSlop.Verification
{
    public static class CatAnimationVerification
    {
        public static Task<string> Run()
        {
            return RunSequence(false);
        }

        public static Task<string> RunIsolated()
        {
            return RunSequence(true);
        }

        private static async Task<string> RunSequence(bool isolated)
        {
            Require(Application.isPlaying, "Run in a fresh Main Play session.");
            var cats = Object.FindObjectsByType<CatController>();
            Require(cats.Length == 2, "The scene must contain exactly two authored cats.");
            var cat = cats.Single(item => item.ContentId == "cat-ground-hall");
            var container = cat.GetComponent<GameObjectContext>().Container;
            var model = isolated ? new CatModel() : container.Resolve<CatModel>();
            var config = AssetDatabase.LoadAssetAtPath<CatConfigSO>("Assets/SO/Cats/Cat.asset");
            var input = isolated ? new MissionInputModel() : container.Resolve<MissionInputModel>();
            if (isolated)
            {
                PrepareIsolated(cat, model, config, input);
            }
            var animator = cat.GetComponentInChildren<Animator>();
            var birds = cat.transform.Find("RecoveryBirds");
            var head = FindBone(animator, "Head");
            var tail = FindBone(animator, "TailBase");
            var paw = FindBone(animator, "FrontLegL");
            var agent = cat.GetComponent<NavMeshAgent>();
            var start = cat.transform.position;
            var deadline = Time.realtimeSinceStartup + 8f;
            while (Time.realtimeSinceStartup < deadline &&
                (!IsState(animator, "Walk") || agent.velocity.magnitude <= 0.05f))
            {
                await Task.Yield();
            }

            Require(IsState(animator, "Walk") && agent.velocity.magnitude > 0.05f,
                "Actual navigation must select Walk.");
            var tailRotation = tail.localRotation;
            var pawRotation = paw.localRotation;
            await Wait(0.18f);
            Require(Quaternion.Angle(tailRotation, tail.localRotation) > 1f,
                "The walking tail must sway.");
            Require(Quaternion.Angle(pawRotation, paw.localRotation) > 1f,
                "Walking must animate paws.");
            Require(Vector3.Distance(start, cat.transform.position) > 0.01f,
                "The agent must actually move while walking.");

            cat.enabled = false;
            cat.GetComponent<CatPatrolController>().enabled = false;
            model.SetThreatened(true);
            await Wait(0.13f);
            Require(IsState(animator, "Idle") && !birds.gameObject.activeSelf,
                "A stopped warning must stop the walking gait and hide stars.");
            model.Advance(config.WarningSeconds);
            Require(model.TryJump(config.WarningSeconds), "The full warning must allow a jump.");
            await Wait(0.13f);
            Require(IsState(animator, "JumpAttack"), "Jumping must select extended attack paws.");
            var attackPaw = paw.localRotation;
            input.SetSuspended(true);
            await Wait(0.05f);
            Require(animator.speed == 0f, "Suspended input must pause the animation.");
            input.SetSuspended(false);
            model.FinishJump();
            await Wait(0.13f);
            Require(IsState(animator, "FaceGrab") && !birds.gameObject.activeSelf,
                "Attachment must select the face-grabbing pose without stars.");
            Require(Quaternion.Angle(attackPaw, paw.localRotation) > 1f,
                "Face grabbing must have a different paw pose from the jump.");
            model.Remove();
            await Wait(0.13f);
            Require(IsState(animator, "JumpAttack"), "Returning must retain an airborne pose.");
            model.FinishReturn();
            await Wait(0.13f);
            Require(IsState(animator, "Stunned") && birds.gameObject.activeSelf,
                "The settled cooldown must show the stunned animation and birds.");
            var headRotation = head.localRotation;
            var orbitRotation = birds.Find("Orbit").localRotation;
            await Wait(0.25f);
            Require(Quaternion.Angle(headRotation, head.localRotation) > 1f,
                "The stunned head must circle.");
            Require(Quaternion.Angle(orbitRotation, birds.Find("Orbit").localRotation) > 10f,
                "The three birds must orbit above the stunned cat.");
            var carryable = cat.GetComponent<CatCarryable>();
            GameObject shoulder = null;
            if (isolated)
            {
                shoulder = new GameObject("Cat animation verification shoulder");
                typeof(Carryable).GetMethod("Attach", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(carryable, new object[] { shoulder.transform });
            }
            else
            {
                carryable.Interact();
            }
            await Wait(0.13f);
            Require(carryable.IsCarried && !birds.gameObject.activeSelf && IsState(animator, "Idle"),
                "Carrying must hide stun birds and stop the dizzy pose.");
            if (isolated)
            {
                Require(model.TryRescue(), "A calm carried cat must remain rescuable.");
                typeof(Carryable).GetMethod("Deliver", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(carryable, new object[] { start });
                Object.Destroy(shoulder);
            }
            else
            {
                Require(carryable.TryRescue(start), "A calm carried cat must remain rescuable.");
            }
            await Wait(0.13f);
            Require(model.State == CatState.Rescued && !birds.gameObject.activeSelf && IsState(animator, "Idle"),
                "A rescued cat must remain calm without stars.");
            Require(Object.FindObjectsByType<CatAnimationView>().Length == 2,
                "Animation must not create duplicate cats or Views.");
            return "PASS: real roaming selects Walk; moving paws and wagging tail; warning idle; attack and face " +
                "poses; return; pause; circling stunned head and orbiting birds; carried/rescued cleanup; two cats.";
        }

        private static void PrepareIsolated(CatController cat, CatModel model, CatConfigSO config,
            MissionInputModel input)
        {
            cat.enabled = false;
            var patrol = cat.GetComponent<CatPatrolController>();
            patrol.enabled = false;
            cat.GetComponent<CatView>().Construct(model);
            cat.GetComponent<CatAnimationView>().Construct(model, config, input);
            cat.GetComponent<CatCarryable>().Construct(model, null, config);
            patrol.Construct(model, new CatPatrolModel(), config, input, new StunModel());
            patrol.enabled = true;
            typeof(CatPatrolController).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(patrol, null);
        }

        public static string CheckSavedPrefab()
        {
            var root = PrefabUtility.LoadPrefabContents("Assets/Prefabs/Characters/Cat.prefab");
            try
            {
                var visual = root.GetComponentInChildren<Animator>();
                Require(visual != null && !visual.applyRootMotion, "The cat requires an in-place Animator.");
                Require(root.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length == 1,
                    "Exactly one skinned body must be saved.");
                Require(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(visual.gameObject) ==
                    "Assets/Models/Characters/CatGinger.fbx", "The visual must remain a connected model instance.");
                Require(!visual.transform.Find("CatRig/StunStars").gameObject.activeSelf,
                    "Stun stars must be hidden in the saved idle prefab.");
                var serialized = new SerializedObject(root.GetComponent<CatView>());
                Require(serialized.FindProperty("_visual").objectReferenceValue == visual.transform,
                    "Encounter motion must reference the new visual.");
                foreach (var clip in visual.runtimeAnimatorController.animationClips)
                {
                    Require(clip.isLooping, "All idle, gait and held poses must loop.");
                    foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                    {
                        Require(string.IsNullOrEmpty(binding.path) || visual.transform.Find(binding.path) != null,
                            "Every animation curve must resolve: " + binding.path);
                    }
                }

                return "PASS: saved connected model, one skinned body, five looping clips, valid bindings, " +
                    "no root motion, hidden stars and correct CatView reference.";
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static Transform FindBone(Animator animator, string name)
        {
            return animator.GetComponentsInChildren<Transform>(true).Single(item => item.name == name);
        }

        private static bool IsState(Animator animator, string name)
        {
            return animator.GetCurrentAnimatorStateInfo(0).IsName(name);
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
