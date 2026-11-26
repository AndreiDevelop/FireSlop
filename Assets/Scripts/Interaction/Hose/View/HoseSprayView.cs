using System;
using UnityEngine;
using Random = System.Random;

namespace FireSlop.Interaction
{
    public sealed class HoseSprayView : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _spray;
        [SerializeField, Min(0f)] private float _spreadSpeed = 0.24f;

        private ParticleSystem.Particle[] _particles;
        private ParticleSystem.Particle[] _birthPoses;
        private long[] _birthIds;
        private Vector3[] _variation;
        private float _rate;
        private int _count;

        private void Awake()
        {
            var main = _spray.main;
            var emission = _spray.emission;
            _rate = emission.rateOverTime.constant;
            emission.enabled = false;
            _particles = new ParticleSystem.Particle[main.maxParticles];
            _variation = new Vector3[main.maxParticles];
            _birthPoses = new ParticleSystem.Particle[main.maxParticles];
            _birthIds = new long[main.maxParticles];
            var random = new Random(739);
            for (var i = 0; i < _variation.Length; i++)
            {
                var angle = (float)random.NextDouble() * 2f * Mathf.PI;
                var drift = _spreadSpeed * Mathf.Sqrt((float)random.NextDouble());
                var size = main.startSize.Evaluate(0f, (float)random.NextDouble());
                _variation[i] = new Vector3(Mathf.Cos(angle) * drift, Mathf.Sin(angle) * drift, size);
                _particles[i].startColor = main.startColor.color;
                _birthIds[i] = -1;
            }
        }

        internal void BeginFrame()
        {
            _count = 0;
        }

        internal void Sample(HoseWaterPacket packet, Vector3 gravity, float age, float flightLifetime)
        {
            var first = (long)Math.Ceiling(packet.LaunchTime * _rate);
            var end = (long)Math.Ceiling((packet.LaunchTime + packet.EmissionDuration) * _rate);
            for (var birth = first; birth < end && _count < _particles.Length; birth++)
            {
                var emissionTime = (float)(birth / (double)_rate - packet.LaunchTime);
                var flightAge = age - emissionTime;
                if (flightAge <= 0f || flightAge >= packet.FlightLimit)
                {
                    continue;
                }

                var slot = (int)(birth % _variation.Length);
                var variation = _variation[slot];
                ref var pose = ref _birthPoses[slot];
                if (_birthIds[slot] != birth)
                {
                    // Capture the launch pose once: turning the nozzle cannot steer a drop already in flight.
                    var blend = Mathf.Clamp01(emissionTime / packet.EmissionDuration);
                    packet.GetEmissionPose(blend, out var origin, out var velocity);
                    var orientation = Quaternion.FromToRotation(Vector3.forward, velocity.normalized);
                    var drift = orientation * new Vector3(variation.x, variation.y, 0f);
                    pose.position = origin;
                    pose.velocity = velocity + drift;
                    _birthIds[slot] = birth;
                }

                var life = flightAge / flightLifetime;
                ref var particle = ref _particles[_count];
                particle.position = pose.position + (pose.velocity + gravity * (0.5f * flightAge)) * flightAge;
                particle.velocity = pose.velocity + gravity * flightAge;
                particle.rotation = slot * 137.5f;
                particle.startLifetime = flightLifetime;
                particle.remainingLifetime = flightLifetime - flightAge;
                var growth = Mathf.SmoothStep(0.22f, 1f, life);
                var fade = Mathf.SmoothStep(0f, 1f, (1f - life) / 0.16f);
                particle.startSize = variation.z * growth * fade;
                _count++;
            }
        }

        internal void DrawFrame()
        {
            if (_count > 0 && !_spray.isPaused)
            {
                _spray.Play(false);
                _spray.Pause(false);
            }

            _spray.SetParticles(_particles, _count);
        }

        internal void Clear()
        {
            _spray.Clear(false);
        }
    }
}
