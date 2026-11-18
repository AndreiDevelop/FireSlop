using UnityEngine;
using Zenject;

namespace FireSlop.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class AudioOutputView : MonoBehaviour
    {
        [SerializeField] private bool _isMusic;

        private SettingsModel _settings;
        private AudioSource _source;
        private float _authoredVolume;
        private bool _subscribed;

        [Inject]
        public void Construct(SettingsModel settings)
        {
            _settings = settings;
            Subscribe();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void Subscribe()
        {
            if (_settings == null || !isActiveAndEnabled || _subscribed)
            {
                return;
            }

            if (_source == null)
            {
                _source = GetComponent<AudioSource>();
                _authoredVolume = _source.volume;
            }

            _settings.LevelsChanged += ApplyVolume;
            _subscribed = true;
            ApplyVolume();
        }

        private void ApplyVolume()
        {
            var level = _isMusic ? _settings.MusicVolume : _settings.EffectsVolume;
            _source.volume = _authoredVolume * level;
        }

        private void OnDisable()
        {
            if (_subscribed)
            {
                _settings.LevelsChanged -= ApplyVolume;
                _subscribed = false;
            }
        }
    }
}
