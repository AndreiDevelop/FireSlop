using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class WaterFireTarget : MonoBehaviour
    {
        [SerializeField] private string _contentId;
        [SerializeField] private Collider _waterTarget;
        [SerializeField] private Collider _passageBlocker;
        [SerializeField] private bool _blocksPassage = true;
        [SerializeField] private ParticleSystem _flames;
        [SerializeField] private WaterFireConfigSO _settings;

        private WaterFireModel _fire;
        private float _fullEmissionRate;

        public string ContentId => _contentId;
        public float Intensity => _fire?.Intensity ?? (_settings != null ? _settings.InitialIntensity : 0f);

        private void Awake()
        {
            if (string.IsNullOrWhiteSpace(_contentId) || _waterTarget == null || _passageBlocker == null ||
                _flames == null || _settings == null || !_settings.IsValid)
            {
                Debug.LogError("Fire source requires a content ID, water target, passage blocker, flames, and valid settings.",
                    this);
                if (_passageBlocker != null)
                {
                    _passageBlocker.enabled = false;
                }

                enabled = false;
                return;
            }

            _fire = new WaterFireModel(
                _settings.SecondsToExtinguish, _settings.InitialIntensity, _settings.PassageOpenIntensity);
            _fullEmissionRate = _flames.emission.rateOverTimeMultiplier;
            Render();
        }

        public void ApplyWater(float seconds)
        {
            if (!isActiveAndEnabled || !_fire.IsBurning)
            {
                return;
            }

            _fire.ApplyWater(seconds);
            Render();
        }

        private void Render()
        {
            var blocksPassage = _blocksPassage && _fire.BlocksPassage;
            if (_passageBlocker.enabled != blocksPassage)
            {
                _passageBlocker.enabled = blocksPassage;
            }

            var emission = _flames.emission;
            emission.rateOverTimeMultiplier = _fullEmissionRate * Intensity;
            if (!_fire.IsBurning)
            {
                _waterTarget.enabled = false;
                _flames.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}
