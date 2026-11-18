using FireSlop.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

namespace FireSlop.MissionHud
{
    [DefaultExecutionOrder(-100)]
    public sealed class AudioSettingsView : MonoBehaviour
    {
        [SerializeField] private GameObject _modal;
        [SerializeField] private Button _open;
        [SerializeField] private Button _close;
        [SerializeField] private Slider _music;
        [SerializeField] private Slider _effects;
        [SerializeField] private TMP_Text _musicPercent;
        [SerializeField] private TMP_Text _effectsPercent;

        private AudioSettingsModel _settings;
        private AudioSettingsController _controller;
        private bool _subscribed;

        [Inject]
        public void Construct(AudioSettingsModel settings, AudioSettingsController controller)
        {
            _settings = settings;
            _controller = controller;
            Subscribe();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void Subscribe()
        {
            if (_controller == null || !isActiveAndEnabled || _subscribed)
            {
                return;
            }

            _subscribed = true;
            _controller.VisibilityChanged += RenderVisibility;
            _open.onClick.AddListener(_controller.Toggle);
            _close.onClick.AddListener(_controller.Toggle);
            RenderVisibility();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (_controller != null && keyboard != null &&
                (keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame))
            {
                _controller.Toggle();
            }
        }

        private void RenderVisibility()
        {
            var isOpen = _controller.IsOpen;
            _modal.SetActive(isOpen);
            if (isOpen)
            {
                _music.onValueChanged.AddListener(_controller.SetMusic);
                _effects.onValueChanged.AddListener(_controller.SetEffects);
                _settings.LevelsChanged += RenderLevels;
                RenderLevels();
                return;
            }

            UnsubscribeFromLevels();
        }

        private void RenderLevels()
        {
            _music.SetValueWithoutNotify(_settings.MusicVolume);
            _effects.SetValueWithoutNotify(_settings.EffectsVolume);
            _musicPercent.text = $"{Mathf.RoundToInt(_settings.MusicVolume * 100f)}%";
            _effectsPercent.text = $"{Mathf.RoundToInt(_settings.EffectsVolume * 100f)}%";
        }

        private void UnsubscribeFromLevels()
        {
            _music.onValueChanged.RemoveListener(_controller.SetMusic);
            _effects.onValueChanged.RemoveListener(_controller.SetEffects);
            _settings.LevelsChanged -= RenderLevels;
        }

        private void OnDisable()
        {
            if (!_subscribed)
            {
                return;
            }

            _controller.Close(resumeMouseLook: false);
            UnsubscribeFromLevels();
            _controller.VisibilityChanged -= RenderVisibility;
            _open.onClick.RemoveListener(_controller.Toggle);
            _close.onClick.RemoveListener(_controller.Toggle);
            _subscribed = false;
        }
    }
}
