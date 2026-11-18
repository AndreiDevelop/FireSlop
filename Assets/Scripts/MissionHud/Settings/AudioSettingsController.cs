using System;
using FireSlop.Audio;
using FireSlop.Firefighter;
using UnityEngine;

namespace FireSlop.MissionHud
{
    public sealed class AudioSettingsController
    {
        private const string MusicKey = "FireSlop.Audio.Music";
        private const string EffectsKey = "FireSlop.Audio.Effects";

        private readonly AudioSettingsModel _settings;
        private readonly MissionInputModel _input;
        private float _previousTimeScale;
        private bool _resumeMouseLook;
        private bool _hasUnsavedLevels;

        public AudioSettingsController(AudioSettingsModel settings, MissionInputModel input)
        {
            _settings = settings;
            _input = input;
            _settings.SetLevels(PlayerPrefs.GetFloat(MusicKey, 1f), PlayerPrefs.GetFloat(EffectsKey, 1f));
        }

        public bool IsOpen { get; private set; }
        public event Action VisibilityChanged;

        public void Toggle()
        {
            if (IsOpen)
            {
                Close();
                return;
            }

            _previousTimeScale = Time.timeScale;
            _resumeMouseLook = Cursor.lockState == CursorLockMode.Locked;
            _input.SetSuspended(true);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            IsOpen = true;
            VisibilityChanged?.Invoke();
        }

        public void Close(bool resumeMouseLook = true)
        {
            if (!IsOpen)
            {
                return;
            }

            Time.timeScale = _previousTimeScale;
            _input.SetSuspended(false);
            if (resumeMouseLook && _resumeMouseLook)
            {
                _input.RequestMouseLook();
            }

            if (_hasUnsavedLevels)
            {
                PlayerPrefs.Save();
                _hasUnsavedLevels = false;
            }

            IsOpen = false;
            VisibilityChanged?.Invoke();
        }

        public void SetMusic(float volume)
        {
            _settings.SetLevels(volume, _settings.EffectsVolume);
            PlayerPrefs.SetFloat(MusicKey, _settings.MusicVolume);
            _hasUnsavedLevels = true;
        }

        public void SetEffects(float volume)
        {
            _settings.SetLevels(_settings.MusicVolume, volume);
            PlayerPrefs.SetFloat(EffectsKey, _settings.EffectsVolume);
            _hasUnsavedLevels = true;
        }
    }
}
