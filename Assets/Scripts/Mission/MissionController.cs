using System;
using FireSlop.Firefighter;
using FireSlop.Interaction;
using FireSlop.MissionHud;
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
        private bool _restarting;

        public MissionController(MissionModel mission, MissionInputModel input, InteractionModel interaction,
            ToolSelectionModel tools)
        {
            _mission = mission;
            _input = input;
            _interaction = interaction;
            _tools = tools;
        }

        public void Initialize()
        {
            _mission.Won += Finish;
            _mission.Begin();
        }

        public void Tick()
        {
            _mission.Advance(Time.unscaledDeltaTime);
        }

        private void Finish()
        {
            _input.SetEnabled(false);
            _interaction.SetActionsEnabled(false);
            _tools.SetEnabled(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Restart()
        {
            if (!_mission.IsWon || _restarting)
            {
                return;
            }

            _restarting = true;
            SceneManager.LoadSceneAsync("Main", LoadSceneMode.Single);
        }

        public void Dispose()
        {
            _mission.Won -= Finish;
        }
    }
}
