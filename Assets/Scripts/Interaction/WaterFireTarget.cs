using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class WaterFireTarget : MonoBehaviour
    {
        [SerializeField] private Collider _waterTarget;
        [SerializeField] private ParticleSystem _flames;
        [SerializeField] private WaterFireConfigSO _settings;

        private WaterFireModel _fire;
        private float _fullEmissionRate;

        public float Intensity => _fire?.Intensity ?? 1f;

        private void Awake()
        {
            if (_waterTarget == null || _flames == null || _settings == null || _settings.SecondsToExtinguish <= 0f)
            {
                Debug.LogError("Water fire target requires a collider, flames, and positive extinguish time.", this);
                enabled = false;
                return;
            }

            _fire = new WaterFireModel(_settings.SecondsToExtinguish);
            _fullEmissionRate = _flames.emission.rateOverTimeMultiplier;
        }

        public void ApplyWater(float seconds)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            _fire.ApplyWater(seconds);
            var emission = _flames.emission;
            emission.rateOverTimeMultiplier = _fullEmissionRate * Intensity;
            if (Intensity <= 0f)
            {
                _waterTarget.enabled = false;
                _flames.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}
