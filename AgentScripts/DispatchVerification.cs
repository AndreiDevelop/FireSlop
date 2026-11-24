using System;
using FireSlop.Cats;
using FireSlop.Dispatch;
using FireSlop.Drone;
using FireSlop.Firefighter;
using FireSlop.Interaction;
using FireSlop.Mission;
using FireSlop.MissionHud;
using FireSlop.Tools;
using FireSlop.Wardrobe;
using UnityEditor;
using UnityEngine;

namespace FireSlop.Verification
{
    public static class DispatchVerification
    {
        public static string Run()
        {
            const string key = "FireSlop.Wardrobe.v1";
            bool hadSave = PlayerPrefs.HasKey(key);
            string previous = PlayerPrefs.GetString(key);
            int checks = 0;
            CursorLockMode originalLock = Cursor.lockState;
            bool originalCursor = Cursor.visible;
            void Require(bool condition, string message)
            {
                if (!condition)
                {
                    throw new InvalidOperationException(message);
                }

                checks++;
            }

            var rewards = AssetDatabase.LoadAssetAtPath<DailyRewardSettingsSO>("Assets/SO/DailyRewards/DailyRewards.asset");
            var ranking = AssetDatabase.LoadAssetAtPath<LeaderboardSettingsSO>("Assets/SO/Leaderboards/LocalRanking.asset");
            var catalog = AssetDatabase.LoadAssetAtPath<WardrobeCatalogSO>("Assets/SO/Wardrobe/FirefighterWardrobe.asset");
            try
            {
                Require(CompactNumber.Format(0) == "0", "Zero display");
                Require(CompactNumber.Format(999) == "999", "Small values stay exact");
                Require(CompactNumber.Format(1000) == "1k", "Thousand suffix");
                Require(CompactNumber.Format(1500) == "1.5k", "Fractional thousand");
                Require(CompactNumber.Format(999950) == "1M", "Rounding promotes suffix");
                Require(CompactNumber.Format(1000000) == "1M", "Million suffix");
                Require(CompactNumber.Format(int.MaxValue) == "2.1B", "Maximum score display");
                Require(CompactNumber.Format(long.MinValue) == "-9.2Qi", "Minimum long display");
                PlayerPrefs.SetString(key, "{\"balance\":37,\"owned\":[\"test-owned\"],\"equipped\":[\"test-equipped\"]}");
                var profile = new WardrobeProfile(catalog);
                DateTime midnight = new DateTime(2026, 11, 24, 23, 59, 59, DateTimeKind.Utc);
                Require(profile.Balance == 37 && profile.DailyClaimCount == 0 && profile.TotalScore == 0, "Old save migration");
                Require(profile.TryClaimDaily(midnight, rewards), "First reward");
                Require(profile.Balance == 42 && profile.DailyClaimCount == 1, "Exact credit");
                Require(!profile.TryClaimDaily(midnight, rewards), "Double claim rejected");
                Require(!profile.TryClaimDaily(midnight.AddDays(-1), rewards), "Clock rollback rejected");
                profile = new WardrobeProfile(catalog);
                Require(profile.Balance == 42 && !profile.CanClaimDaily(midnight), "Reload preserves claim");
                Require(profile.TryClaimDaily(midnight.AddSeconds(1), rewards), "UTC midnight boundary");
                Require(profile.TryClaimDaily(midnight.AddDays(4), rewards), "Missed dates preserve next day");
                for (int day = 3; day < 7; day++)
                {
                    Require(profile.TryClaimDaily(midnight.AddDays(day + 4), rewards), "Remaining reward " + day);
                }

                Require(profile.Balance == 200 && profile.DailyClaimCount == 7, "Seven-day exact total 163");
                Require(profile.TotalScore == 0, "Daily rewards do not increase ranking score");
                Require(!profile.TryClaimDaily(midnight.AddYears(1), rewards), "Completed track cannot repeat");
                using (var leaderboard = new LeaderboardModel(ranking, profile))
                {
                    Require(leaderboard.Count == ranking.RivalCount + 1, "Authored rivals plus player");
                    Require(leaderboard.PlayerRank == 10, "Initial player rank");
                    profile.CreditMissionSuccess(ranking.ScorePerMission);
                    Require(profile.TotalScore == 100 && leaderboard.PlayerRank == 9, "Mission score and tie ordering");
                    Require(profile.Balance == 200 + catalog.RescueReward, "Currency and score commit together");
                    Require(leaderboard.EntryAt(8).IsPlayer, "Sorted player row matches pinned rank");
                    profile.CreditMissionSuccess(2400);
                    Require(profile.TotalScore == 2500 && leaderboard.PlayerRank == 1, "Player reaches podium first place");
                    Require(leaderboard.EntryAt(0).IsPlayer, "Podium uses sorted entries");
                }

                profile = new WardrobeProfile(catalog);
                Require(profile.TotalScore == 2500, "Score persists across reload");
                string saved = PlayerPrefs.GetString(key);
                Require(saved.Contains("test-owned") && saved.Contains("test-equipped"), "Ownership and equipment preserved");
                var mission = new MissionModel("one", "two", "three");
                int wins = 0;
                void CountWin()
                {
                    wins++;
                }

                mission.Won += CountWin;
                mission.Begin();
                mission.Advance(12d);
                mission.TryRescue("one");
                mission.TryRescue("two");
                Require(!mission.IsWon && wins == 0, "Partial mission cannot award score");
                mission.TryRescue("three");
                mission.TryRescue("three");
                Require(mission.IsWon && wins == 1, "Successful mission awards once");
                mission.Advance(7d);
                Require(mission.ElapsedSeconds == 12d, "Mission timer still freezes at victory");
                var belt = AssetDatabase.LoadAssetAtPath<ToolBeltSO>("Assets/SO/ToolBelts/ToolBelt.asset");
                var tools = new ToolSelectionModel(belt);
                var input = new MissionInputModel();
                using (var interaction = new InteractionModel(tools, input, new DroneModel(), new CatFaceModel()))
                {
                    var pendingMission = new MissionModel("a", "b", "c");
                    var controller = new MissionController(pendingMission, input, interaction, tools, null, profile, ranking);
                    var finish = typeof(MissionController).GetMethod("Finish", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    finish.Invoke(controller, null);
                    Require(profile.TotalScore == 2500, "Production controller blocks uncompleted mission reward");
                    pendingMission.Begin();
                    pendingMission.TryRescue("a");
                    pendingMission.TryRescue("b");
                    pendingMission.TryRescue("c");
                    finish.Invoke(controller, null);
                    Require(profile.TotalScore == 2600 && !input.IsEnabled, "Production victory awards score and blocks input");
                    finish.Invoke(controller, null);
                    Require(profile.TotalScore == 2600, "Production duplicate victory cannot award again");
                    controller.Dispose();
                }
                PlayerPrefs.SetString(key, "{\"balance\":2147483647}");
                profile = new WardrobeProfile(catalog);
                bool overflow = false;
                try
                {
                    profile.TryClaimDaily(midnight, rewards);
                }
                catch (OverflowException)
                {
                    overflow = true;
                }

                Require(overflow && profile.DailyClaimCount == 0, "Overflow preserves eligible daily reward");
                PlayerPrefs.SetString(key, "{\"balance\":90,\"dailyClaimCount\":9}");
                profile = new WardrobeProfile(catalog);
                Require(profile.LoadError != null && !profile.CanClaimDaily(midnight), "Corrupt progress blocked");
                Require(PlayerPrefs.GetString(key).Contains("90"), "Invalid save not replaced");
                PlayerPrefs.SetString(key, "{\"totalScore\":2147483640}");
                profile = new WardrobeProfile(catalog);
                profile.CreditMissionSuccess(ranking.ScorePerMission);
                Require(profile.TotalScore == int.MaxValue, "Ranking score cannot wrap negative");
            }
            finally
            {
                if (hadSave)
                {
                    PlayerPrefs.SetString(key, previous);
                }
                else
                {
                    PlayerPrefs.DeleteKey(key);
                }

                PlayerPrefs.Save();
                Cursor.lockState = originalLock;
                Cursor.visible = originalCursor;
            }

            return checks + " profile, calendar, score and ranking checks passed; original save restored.";
        }
    }
}
