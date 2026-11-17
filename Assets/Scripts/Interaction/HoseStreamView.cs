using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class HoseStreamView : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _droplets;
        [SerializeField] private ParticleSystem _splash;

        private ParticleSystem.Particle[] _particles;
        private ParticleSystem.Particle[] _dropletSamples;
        private int _count;
        private float _dropletRate;
        private float _dropletRemainder;
        private float _splashRate;
        private bool _refreshSettings = true;

        private void Awake()
        {
            if (_droplets == null || _splash == null)
            {
                Debug.LogError("Hose stream requires authored droplet and splash particle systems.", this);
                enabled = false;
                return;
            }

            // Keep the Particle System Inspector preview available while gameplay owns all emission.
            var dropletEmission = _droplets.emission;
            dropletEmission.enabled = false;

            var splashEmission = _splash.emission;
            _splashRate = splashEmission.rateOverTime.constant;
            splashEmission.enabled = false;
        }

        internal int TakeDroplets(float sampleDuration)
        {
            if (!isActiveAndEnabled)
            {
                return 0;
            }

            if (_refreshSettings)
            {
                ReadDropletSettings();
                _refreshSettings = false;
            }

            var emission = Mathf.Min(_dropletRate * sampleDuration + _dropletRemainder, _particles.Length);
            var count = Mathf.FloorToInt(emission);
            _dropletRemainder = emission - count;
            return count;
        }

        private void ReadDropletSettings()
        {
            var main = _droplets.main;
            var shape = _droplets.shape;
            var maxParticles = main.maxParticles;
            if (_particles == null || _particles.Length != maxParticles)
            {
                _particles = new ParticleSystem.Particle[maxParticles];
                _dropletSamples = new ParticleSystem.Particle[maxParticles];
            }

            var radius = shape.enabled ? shape.radius : 0f;
            var innerRadius = 1f - shape.radiusThickness;
            var coneAngle = shape.enabled ? shape.angle * Mathf.Deg2Rad : 0f;
            for (var index = 0; index < _dropletSamples.Length; index++)
            {
                var randomRadius = HashFraction(index, 0x9E3779B9u);
                var radialDistance = Mathf.Sqrt(Mathf.Lerp(innerRadius * innerRadius, 1f, randomRadius));
                var angle = HashFraction(index, 0x85EBCA6Bu) * shape.arc * Mathf.Deg2Rad;
                var radialDirection = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                var launchAngle = coneAngle * radialDistance;
                _dropletSamples[index] = new ParticleSystem.Particle
                {
                    position = radialDirection * (radius * radialDistance),
                    velocity = radialDirection * Mathf.Sin(launchAngle) + Vector3.forward * Mathf.Cos(launchAngle),
                    startSize = main.startSize.Evaluate(0f, HashFraction(index, 0xC2B2AE35u)),
                    startColor = main.startColor.Evaluate(0f, HashFraction(index, 0x27D4EB2Fu))
                };
            }

            _dropletRate = Mathf.Max(0f, _droplets.emission.rateOverTime.Evaluate(0f, 0.5f));
        }

        private static float HashFraction(int index, uint salt)
        {
            var hash = unchecked(((uint)index + salt) * 0x45D9F3Bu);
            hash ^= hash >> 16;
            return (hash & 0xFFFFu) / 65536f;
        }

        internal void BeginFrame()
        {
            _count = 0;
        }

        internal void DrawPacket(HoseWaterPacket packet, Vector3 gravity, float age, float sampleDuration)
        {
            if (!isActiveAndEnabled || packet.DropletCount == 0)
            {
                return;
            }

            var rotation = Quaternion.LookRotation(packet.LaunchVelocity);
            var speed = packet.LaunchVelocity.magnitude;
            var sampleIndex = (packet.LaunchTime.GetHashCode() & int.MaxValue) % _dropletSamples.Length;
            for (var index = 0; index < packet.DropletCount && _count < _particles.Length; index++)
            {
                var sample = _dropletSamples[sampleIndex];
                sampleIndex = (sampleIndex + 1) % _dropletSamples.Length;
                var phase = (float)index / packet.DropletCount;
                var dropletAge = age - phase * sampleDuration;
                if (dropletAge < 0f || dropletAge > packet.FlightLimit)
                {
                    continue;
                }

                var launchVelocity = rotation * sample.velocity * speed;
                sample.position = packet.Origin + rotation * sample.position +
                    (launchVelocity + gravity * (0.5f * dropletAge)) * dropletAge;
                sample.velocity = launchVelocity + gravity * dropletAge;
                sample.startLifetime = packet.FlightLimit;
                sample.remainingLifetime = Mathf.Max(float.Epsilon, packet.FlightLimit - dropletAge);
                _particles[_count] = sample;
                _count++;
            }
        }

        internal void DrawFrame()
        {
            if (!isActiveAndEnabled || _particles == null)
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
            _dropletRemainder = 0f;
            _refreshSettings = true;
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
