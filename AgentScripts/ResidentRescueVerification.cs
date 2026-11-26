using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FireSlop.Firefighter;
using FireSlop.Interaction;
using FireSlop.Mission;
using FireSlop.MissionHud;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Object = UnityEngine.Object;

namespace FireSlop.Verification
{
    public static class ResidentRescueVerification
    {
        public static string CheckModel()
        {
            var ids = new[] { "first", "second", "third" };
            var mission = new MissionModel(ids);
            ids[0] = "changed";
            Require(mission.ResidentIds[0] == "first", "Caller mutations must not change mission IDs.");
            Require(!mission.TryRescue("first"), "Rescue before mission start must fail.");
            mission.RevealResident("second");
            Require(mission.IsRevealed("second") && !mission.IsRevealed("first"),
                "Opening one cabinet must not reveal another resident.");
            var wins = 0;
            mission.Won += () => wins++;
            mission.Begin();
            Require(!mission.TryRescue(null) && !mission.TryRescue("cat"), "Unknown targets must not count.");
            foreach (var id in mission.ResidentIds)
            {
                mission.Advance(1d);
                Require(mission.TryRescue(id), "Each resident must count once.");
                Require(!mission.TryRescue(id), "Repeated contact must not count again.");
                if (mission.RescuedCount < 3)
                {
                    Require(mission.IsActive && !mission.IsWon && wins == 0,
                        "The first two rescues must keep the mission running.");
                }
            }

            Require(mission.IsWon && !mission.IsActive && wins == 1, "Only the third rescue must win.");
            mission.Advance(10d);
            mission.Begin();
            Require(mission.ElapsedSeconds == 3d && !mission.IsActive, "Completion must freeze time permanently.");
            var fresh = new MissionModel("first", "second", "third");
            Require(fresh.RescuedCount == 0 && fresh.ElapsedSeconds == 0d && !fresh.IsRevealed("second"),
                "A restart must not retain mission progress.");
            var rejected = false;
            try
            {
                new MissionModel("duplicate", "duplicate");
            }
            catch (ArgumentException)
            {
                rejected = true;
            }

            Require(rejected, "Duplicate authored IDs must fail validation.");
            return "PASS: independent reveal, unique IDs, duplicate contact, third-rescue victory and frozen timer.";
        }

        public static string CheckAuthoring()
        {
            Require(!EditorApplication.isPlaying, "Authoring checks require Edit Mode.");
            var residents = Object.FindObjectsByType<ResidentView>();
            Require(residents.Length == 3, "Main must contain exactly three visible authored residents.");
            Require(residents.Select(x => x.ContentId).Distinct().Count() == 3, "Resident IDs must be unique.");
            foreach (var resident in residents)
            {
                Require(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(resident) ==
                    "Assets/Prefabs/Characters/Resident/RescueResident.prefab", "Each resident must remain connected.");
                var pose = (GameObject)new SerializedObject(resident).FindProperty("_carryPose").objectReferenceValue;
                Require(pose != null && !pose.activeSelf && pose.transform.parent == resident.transform &&
                    pose.GetComponentInChildren<Rigidbody>(true) == null, "Carry poses must be static resident children.");
            }

            var carrier = new SerializedObject(Object.FindAnyObjectByType<RescueController>());
            Require(carrier.FindProperty("_residents").arraySize == 3, "Carrier must reference all residents.");
            var cabinets = carrier.FindProperty("_cabinets");
            Require(cabinets.arraySize == 5, "All five approved hiding spots must remain available.");
            return "PASS: three connected resident instances, unique IDs, own carry rigs and five hiding spots.";
        }

        public static async Task<string> CheckRun()
        {
            var container = FindContainer();
            var mission = container.Resolve<MissionModel>();
            var input = container.Resolve<InteractionModel>();
            var playerInput = container.Resolve<MissionInputModel>();
            var carrier = container.Resolve<RescueController>();
            var quest = container.Resolve<QuestTargetModel>();
            var residents = container.ResolveAll<ResidentView>();
            Require(mission.RequiredRescues == 3 && mission.RescuedCount == 0, "Start a fresh three-resident run.");
            Require(residents.Select(x => x.transform.position).Distinct().Count() == 3,
                "Each resident must occupy a different hiding spot.");
            var firstFrame = Time.frameCount;
            var firstTime = mission.ElapsedSeconds;
            await WaitSeconds(0.15f);
            Require(Time.frameCount > firstFrame && mission.ElapsedSeconds > firstTime, "Frames and timer must advance.");
            var player = Object.FindAnyObjectByType<FirefighterController>();
            var character = player.GetComponent<CharacterController>();
            player.enabled = false;
            character.enabled = false;
            var camera = container.Resolve<Camera>();
            var zones = Object.FindObjectsByType<RescueZoneView>();
            var groundZone = zones.Single(x => x.ContentId == "main-rescue-zone-01");
            var trampoline = zones.Single(x => x != groundZone);
            Require(!residents[0].GetPrompt(residents[0].PickupCollider).IsAvailable,
                "A closed occupied cabinet must block pickup.");
            var firstCabinet = GetCabinet(residents[0]);
            firstCabinet.GetComponent<CabinetContextTarget>().Interact();
            await WaitSeconds(0.8f);
            Require(mission.IsRevealed(residents[0].ContentId) && !mission.IsRevealed(residents[1].ContentId),
                "Opening one occupied cabinet must reveal only its resident.");
            for (var index = 0; index < residents.Count; index++)
            {
                var resident = residents[index];
                var cabinet = GetCabinet(resident);
                if (!cabinet.IsOpen)
                {
                    cabinet.GetComponent<CabinetContextTarget>().Interact();
                    await WaitSeconds(0.8f);
                }

                Require(resident.GetPrompt(resident.PickupCollider).IsAvailable,
                    "A fully open cabinet must advertise pickup for its resident.");
                resident.Interact();
                Require(carrier.Carried == resident && input.CarriedContentId == resident.ContentId,
                    "The shared pickup receiver must carry the selected resident.");
                Require(quest.Step == QuestStep.DeliverResident, "Every carried resident must target the rescue zone.");
                if (index == 0)
                {
                    camera.transform.rotation = Quaternion.identity;
                    player.transform.position = new Vector3(-24f, -0.76f, 12f);
                    Physics.SyncTransforms();
                    input.BeginPrimary(PrimaryInputSource.Touch);
                    input.EndPrimary(PrimaryInputSource.Touch);
                    Require(!input.IsCarrying, "The shared throw action must free the shoulder.");
                    var body = resident.GetComponent<Rigidbody>();
                    var volume = trampoline.GetComponent<BoxCollider>();
                    body.position = volume.bounds.center;
                    body.linearVelocity = Vector3.zero;
                    resident.transform.position = body.position;
                }
                else
                {
                    var volume = groundZone.GetComponent<BoxCollider>();
                    player.transform.position += volume.bounds.center - resident.DeliveryPosition;
                }

                Physics.SyncTransforms();
                await WaitSeconds(0.25f);
                Require(mission.RescuedCount == index + 1 && resident.IsRescued && !input.IsCarrying,
                    "Actual rescue volume contact must count once and free the shoulder.");
                Require(!resident.TryRescue(resident.DeliveryPosition) &&
                    !resident.GetPrompt(resident.PickupCollider).IsAvailable,
                    "A rescued resident must reject duplicate delivery and pickup.");
                if (index < residents.Count - 1)
                {
                    Require(mission.IsActive && !mission.IsWon && playerInput.IsEnabled,
                        "Partial progress must leave gameplay and timing active.");
                    Require(quest.RescueProgress == $"{index + 1}/3" && quest.TargetResidentId != resident.ContentId,
                        "Progress and arrow target must advance past rescued residents.");
                    var text = Object.FindAnyObjectByType<QuestArrowView>().GetComponentsInChildren<TMP_Text>(true);
                    Require(text.Any(x => x.text.StartsWith(quest.RescueProgress)), "The live HUD must display progress.");
                }
            }

            Require(mission.IsWon && !playerInput.IsEnabled, "The third rescue must stop gameplay.");
            var finishTime = mission.ElapsedSeconds;
            await WaitSeconds(0.2f);
            Require(mission.ElapsedSeconds == finishTime, "The final elapsed time must stay frozen.");
            Require(Object.FindAnyObjectByType<MissionResultView>().GetComponentsInChildren<TMP_Text>(true)
                .Any(x => x.gameObject.activeInHierarchy && x.text == "3/3 residents rescued!"),
                "The authored result must show all three rescues.");
            return "PASS: ticking frames, independent cabinets, pickup, touch throw, trampoline, carried exit delivery, " +
                "1/3 and 2/3 HUD, free shoulder, duplicate rejection, third-rescue result and frozen time.";
        }

        public static async Task<string> CheckRestart()
        {
            var previous = FindContainer().Resolve<MissionModel>();
            Require(previous.IsWon, "Complete CheckRun before checking restart.");
            var result = Object.FindAnyObjectByType<MissionResultView>();
            var button = (Button)new SerializedObject(result).FindProperty("_restart").objectReferenceValue;
            button.onClick.Invoke();
            button.onClick.Invoke();
            await WaitSeconds(1.5f);
            var container = FindContainer();
            var mission = container.Resolve<MissionModel>();
            Require(!ReferenceEquals(previous, mission) && mission.IsActive && !mission.IsWon &&
                mission.RescuedCount == 0 && mission.ElapsedSeconds < 5d, "Restart must reset progress and timer.");
            var residents = container.ResolveAll<ResidentView>();
            Require(residents.Count == 3 && Object.FindObjectsByType<ResidentView>().Length == 3,
                "Restart must not duplicate residents.");
            Require(residents.Select(x => x.transform.position).Distinct().Count() == 3,
                "Restart must choose three distinct hiding spots.");
            Require(residents.All(x => !GetCabinet(x).IsOpen && !x.IsRescued && !mission.IsRevealed(x.ContentId)),
                "Restart must restore closed occupied cabinets and unrevealed residents.");
            Require(!container.Resolve<InteractionModel>().IsCarrying, "Restart must restore free hands.");
            return "PASS: authored restart button, fresh model/timer, closed cabinets, distinct hiding spots, " +
                "three residents and free hands.";
        }

        private static Openable GetCabinet(ResidentView resident)
        {
            return (Openable)typeof(ResidentView).GetField("_cabinet",
                BindingFlags.Instance | BindingFlags.NonPublic).GetValue(resident);
        }

        private static DiContainer FindContainer()
        {
            return Object.FindObjectsByType<SceneContext>().Single(x => x.gameObject.scene.name == "Main").Container;
        }

        private static async Task WaitSeconds(float seconds)
        {
            var until = EditorApplication.timeSinceStartup + seconds;
            while (EditorApplication.timeSinceStartup < until)
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
