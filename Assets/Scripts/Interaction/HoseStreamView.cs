using UnityEngine;
using Zenject;

namespace FireSlop.Interaction
{
    public sealed class HoseStreamView : MonoBehaviour
    {
        [SerializeField] private LineRenderer _stream;
        [SerializeField] private ParticleSystem _droplets;
        [SerializeField] private ParticleSystem _splash;

        private InteractionModel _model;
        private float _dropletSpeed;

        private void Awake()
        {
            if (_stream == null || _droplets == null || _splash == null ||
                _droplets.main.startSpeed.mode != ParticleSystemCurveMode.Constant ||
                _droplets.main.startSpeed.constant <= 0f)
            {
                Debug.LogError("Hose water requires a line, two particle systems, and a positive droplet speed.", this);
                enabled = false;
                return;
            }

            _dropletSpeed = _droplets.main.startSpeed.constant;
        }

        [Inject]
        public void Construct(InteractionModel model)
        {
            _model = model;
            if (isActiveAndEnabled)
            {
                _model.HoseStopped += Hide;
            }
        }

        private void OnEnable()
        {
            if (_model != null)
            {
                _model.HoseStopped += Hide;
            }

            Hide();
        }

        private void OnDisable()
        {
            if (_model != null)
            {
                _model.HoseStopped -= Hide;
            }

            Hide();
        }

        public void Render(Vector3 origin, Vector3 end, Vector3 surfaceNormal)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            _stream.SetPosition(0, origin);
            _stream.SetPosition(1, end);
            _stream.enabled = true;
            RenderDroplets(origin, end);

            if (surfaceNormal == Vector3.zero)
            {
                if (_splash.isEmitting)
                {
                    _splash.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                }

                return;
            }

            _splash.transform.SetPositionAndRotation(end, Quaternion.FromToRotation(Vector3.forward, surfaceNormal));
            if (!_splash.isEmitting)
            {
                _splash.Play(false);
            }
        }

        private void RenderDroplets(Vector3 origin, Vector3 end)
        {
            var travel = end - origin;
            if (travel.sqrMagnitude <= Mathf.Epsilon)
            {
                _droplets.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                return;
            }

            _droplets.transform.SetPositionAndRotation(origin, Quaternion.LookRotation(travel));
            var settings = _droplets.main;
            settings.startLifetime = travel.magnitude / _dropletSpeed;
            if (!_droplets.isEmitting)
            {
                _droplets.Play(false);
            }
        }

        public void Hide()
        {
            if (_stream != null)
            {
                _stream.enabled = false;
            }

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
