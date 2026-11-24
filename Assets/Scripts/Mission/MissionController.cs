using System;
using FireSlop.Dispatch;
using FireSlop.Firefighter;
using FireSlop.Interaction;
using FireSlop.MissionHud;
using FireSlop.Wardrobe;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace FireSlop.Mission
{
    public sealed class MissionController : IInitializable, ITickable, IDisposable
    {
        private readonly MissionModel _mission;
        private readonly MissionInputModel _input;
        private readonly InteractionModel _interaction;
        private readonly ToolSelectionModel _tools;
        private readonly RescueController _rescue;
        private readonly WardrobeProfile _profile;
        private readonly LeaderboardSettingsSO _leaderboard;
        private bool _rewarded;
        private bool _restarting;

        public MissionController(MissionModel mission, MissionInputModel input, InteractionModel interaction,
            ToolSelectionModel tools, RescueController rescue, WardrobeProfile profile, LeaderboardSettingsSO leaderboard)
        {
            _profile = profile;
            _leaderboard = leaderboard;
            _mission = mission;
            _input = input;
            _interaction = interaction;
            _tools = tools;
            _rescue = rescue;
        }

        public void Initialize()
        {
            _mission.Won += Finish;
            _rescue.PlaceResidents();
            _mission.Begin();
        }

        public void Tick()
        {
            if (Time.timeScale > 0f)
            {
                _mission.Advance(Time.unscaledDeltaTime);
            }
        }

        private void Finish()
        {
            if (!_mission.IsWon || _rewarded)
            {
                return;
            }

            _rewarded = true;
            _input.SetEnabled(false);
            _interaction.SetActionsEnabled(false);
            _tools.SetEnabled(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            try
            {
                _profile.CreditMissionSuccess(_leaderboard.ScorePerMission);
            }
            catch (Exception error)
            {
                Debug.LogError("Mission result profile save failed: " + error.Message);
            }
        }

        public void Restart()
        {
            LeaveResult("Main");
        }

        public void ReturnToMenu()
        {
            LeaveResult("Intro");
        }

        private void LeaveResult(string scene)
        {
            if (!_mission.IsWon || _restarting)
            {
                return;
            }

            _restarting = true;
            SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
        }

        public void Dispose()
        {
            _mission.Won -= Finish;
        }
    }
}
