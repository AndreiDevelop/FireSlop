using System.Collections;
using UnityEngine;
using Zenject;

namespace FireSlop.Interaction
{
    public sealed class WaterFireTarget : MonoBehaviour
    {
        [SerializeField] private string _contentId;
        [SerializeField] private Collider _waterTarget;
        [SerializeField] private Collider _passageBlocker;
        [SerializeField] private bool _blocksPassage = true;
        [SerializeField] private ParticleSystem _flames;
        [SerializeField] private ParticleSystem _smoke;
        [SerializeField] private WaterFireConfigSO _settings;

        [SerializeField] private AudioSource _fireAudio;

        private WaterFireModel _fire;
        private float _fullEmissionRate;
        private float _reignitionDelay;
        private double _smokeUntil;
        private Coroutine _smokeRoutine;

        public string ContentId => _contentId;
        public float Intensity => _fire?.Intensity ?? (_settings != null ? _settings.InitialIntensity : 0f);
        internal WaterFireModel Fire => _fire;
        internal WaterFireConfigSO Settings => _settings;

        [Inject]
        internal void Construct([InjectOptional] WaterFireModel fire, [InjectOptional] FireSpreadConfigSO spread)
        {
            _fire = fire;
            if (spread != null)
            {
                _reignitionDelay = spread.ReignitionDelay;
            }
        }

        private void Awake()
        {
            if ((_fire == null && string.IsNullOrWhiteSpace(_contentId)) ||
                _waterTarget == null || _passageBlocker == null ||
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

            _fire ??= new WaterFireModel(
                _settings.SecondsToExtinguish, _settings.InitialIntensity, _settings.PassageOpenIntensity);
            _fullEmissionRate = _flames.emission.rateOverTimeMultiplier;
            Render();
        }

        internal void EnableReignition(float delay)
        {
            _reignitionDelay = delay;
        }

        public void ApplyWater(float seconds)
        {
            if (!isActiveAndEnabled || !_fire.IsBurning || !(seconds > 0f))
            {
                return;
            }

            var time = Time.timeAsDouble;
            _fire.ApplyWater(seconds, time, _reignitionDelay);
            ShowSmoke(time + seconds + Time.fixedDeltaTime);
            Render();
        }

        private void OnEnable()
        {
            if (_fire != null)
            {
                UpdateFireAudio(_fire.IsBurning);
            }
        }

        private void OnDisable()
        {
            if (_fireAudio != null)
            {
                _fireAudio.Stop();
            }

            if (_smokeRoutine == null)
            {
                return;
            }

            StopCoroutine(_smokeRoutine);
            _smokeRoutine = null;
            _smoke.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void ShowSmoke(double until)
        {
            if (_smoke == null)
            {
                return;
            }

            _smokeUntil = until;
            if (_smokeRoutine == null)
            {
                _smoke.Play(false);
                _smokeRoutine = StartCoroutine(StopSmokeWhenDry());
            }
        }

        private IEnumerator StopSmokeWhenDry()
        {
            while (Time.timeAsDouble < _smokeUntil)
            {
                yield return null;
            }

            _smoke.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            _smokeRoutine = null;
        }

        internal void Render()
        {
            var blocksPassage = _blocksPassage && _fire.BlocksPassage;
            if (_passageBlocker.enabled != blocksPassage)
            {
                _passageBlocker.enabled = blocksPassage;
            }

            var emission = _flames.emission;
            emission.rateOverTimeMultiplier = _fullEmissionRate * Intensity;
            var burns = _fire.IsBurning;
            _waterTarget.enabled = burns;
            UpdateFireAudio(burns);
            if (burns && !_flames.isEmitting)
            {
                _flames.Play(false);
            }
            else if (!burns && _flames.isEmitting)
            {
                _flames.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        private void UpdateFireAudio(bool burns)
        {
            if (_fireAudio == null || _fireAudio.clip == null || _fireAudio.isPlaying == burns)
            {
                return;
            }

            if (burns && isActiveAndEnabled)
            {
                _fireAudio.Play();
            }
            else
            {
                _fireAudio.Stop();
            }
        }
    }
}
