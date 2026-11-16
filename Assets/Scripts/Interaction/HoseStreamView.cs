using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class HoseStreamView : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _droplets;
        [SerializeField] private ParticleSystem _splash;
        [SerializeField, Range(1, 32)] private int _dropletsPerSample;
        [SerializeField, Min(0f)] private float _dropletSpread;

        private ParticleSystem.Particle[] _particles;
        private int _count;
        private Vector2[] _offsets;
        private float[] _sizes;
        private Color32[] _colors;
        private float _splashRate;

        private void Awake()
        {
            if (_droplets == null || _splash == null || _dropletsPerSample <= 0)
            {
                Debug.LogError("Hose stream requires authored droplet and splash particle systems.", this);
                enabled = false;
                return;
            }

            var settings = _droplets.main;
            _particles = new ParticleSystem.Particle[settings.maxParticles];
            _offsets = new Vector2[_dropletsPerSample];
            _sizes = new float[_dropletsPerSample];
            _colors = new Color32[_dropletsPerSample];
            for (var index = 0; index < _dropletsPerSample; index++)
            {
                var phase = (float)index / _dropletsPerSample;
                var angle = phase * Mathf.PI * 8f;
                _offsets[index] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (_dropletSpread * phase);
                _sizes[index] = settings.startSize.Evaluate(phase, phase);
                _colors[index] = settings.startColor.Evaluate(phase, phase);
            }

            // Keep the Particle System Inspector preview available while gameplay owns all emission.
            var dropletEmission = _droplets.emission;
            dropletEmission.enabled = false;

            var splashEmission = _splash.emission;
            _splashRate = splashEmission.rateOverTime.constant;
            splashEmission.enabled = false;
        }

        internal void BeginFrame()
        {
            _count = 0;
        }

        internal void DrawPacket(HoseWaterPacket packet, Vector3 gravity, float age, float sampleDuration)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            var forward = packet.LaunchVelocity.normalized;
            var right = Vector3.Cross(Vector3.up, forward).normalized;
            var up = Vector3.Cross(forward, right);
            for (var index = 0; index < _dropletsPerSample && _count < _particles.Length; index++)
            {
                var phase = (float)index / _dropletsPerSample;
                var dropletAge = age - phase * sampleDuration;
                if (dropletAge < 0f || dropletAge > packet.FlightLimit)
                {
                    continue;
                }

                var offset = right * _offsets[index].x + up * _offsets[index].y;
                _particles[_count] = new ParticleSystem.Particle
                {
                    position = packet.PositionAt(dropletAge, gravity) + offset,
                    velocity = packet.VelocityAt(dropletAge, gravity),
                    startSize = _sizes[index],
                    startColor = _colors[index],
                    startLifetime = packet.FlightLimit,
                    remainingLifetime = Mathf.Max(float.Epsilon, packet.FlightLimit - dropletAge)
                };
                _count++;
            }
        }

        internal void DrawFrame()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (_droplets.isStopped)
            {
                _droplets.Play(false);
                _droplets.Pause(false);
            }

            _droplets.SetParticles(_particles, _count);
        }

        public void Splash(Vector3 point, Vector3 normal, float sampleDuration)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            _splash.transform.SetPositionAndRotation(point, Quaternion.FromToRotation(Vector3.forward, normal));
            _splash.Emit(Mathf.Max(1, Mathf.RoundToInt(_splashRate * sampleDuration)));
        }

        public void Hide()
        {
            if (_droplets != null)
            {
                _droplets.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            if (_splash != null)
            {
                _splash.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
}
