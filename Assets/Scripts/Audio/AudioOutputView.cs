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
        private float _gain = 1f;

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

            CacheSource();

            _settings.LevelsChanged += ApplyVolume;
            _subscribed = true;
            ApplyVolume();
        }

        private void ApplyVolume()
        {
            var level = _settings == null ? 1f : (_isMusic ? _settings.MusicVolume : _settings.EffectsVolume);
            _source.volume = _authoredVolume * level * _gain;
        }

        public void SetGain(float gain)
        {
            CacheSource();
            _gain = Mathf.Clamp01(gain);
            ApplyVolume();
        }

        private void CacheSource()
        {
            if (_source != null)
            {
                return;
            }

            _source = GetComponent<AudioSource>();
            _authoredVolume = _source.volume;
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
