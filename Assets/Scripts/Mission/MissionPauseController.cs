using System;
using FireSlop.Firefighter;
using UnityEngine;

namespace FireSlop.Mission
{
    public sealed class MissionPauseController : IDisposable
    {
        private readonly MissionInputModel _input;
        private bool _settingsOpen;
        private bool _paused;
        private float _previousTimeScale;

        public MissionPauseController(MissionInputModel input)
        {
            _input = input;
        }

        public bool IsOrientationBlocked { get; private set; }

        public void SetOrientationBlocked(bool blocked)
        {
            IsOrientationBlocked = blocked;
            ApplyPause();
        }

        public void SetSettingsOpen(bool isOpen)
        {
            _settingsOpen = isOpen;
            ApplyPause();
        }

        private void ApplyPause()
        {
            var shouldPause = _settingsOpen || IsOrientationBlocked;
            if (_paused == shouldPause)
            {
                return;
            }

            _paused = shouldPause;
            if (_paused)
            {
                _previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = _previousTimeScale;
            }

            _input.SetSuspended(_paused);
        }

        public void Dispose()
        {
            _settingsOpen = false;
            IsOrientationBlocked = false;
            ApplyPause();
        }
    }
}
