using System;
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
        private const string HasStartedKey = "FireSlop.Mission.HasStarted";

        private readonly MissionModel _mission;
        private readonly MissionInputModel _input;
        private readonly InteractionModel _interaction;
        private readonly ToolSelectionModel _tools;
        private readonly RescueController _rescue;
        private readonly WardrobeProfile _profile;
        private bool _restarting;

        public MissionController(MissionModel mission, MissionInputModel input, InteractionModel interaction,
            ToolSelectionModel tools, RescueController rescue, WardrobeProfile profile)
        {
            _profile = profile;
            _mission = mission;
            _input = input;
            _interaction = interaction;
            _tools = tools;
            _rescue = rescue;
        }

        public void Initialize()
        {
            _mission.Won += Finish;
            var isFirstRun = PlayerPrefs.GetInt(HasStartedKey) == 0;
            _rescue.PlaceResident(isFirstRun);
            _mission.Begin();
            if (isFirstRun)
            {
                PlayerPrefs.SetInt(HasStartedKey, 1);
                PlayerPrefs.Save();
            }
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
            _profile.CreditRescue();
            _input.SetEnabled(false);
            _interaction.SetActionsEnabled(false);
            _tools.SetEnabled(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
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
