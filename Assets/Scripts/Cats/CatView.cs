using System;
using FireSlop.Interaction;
using UnityEngine;
using Zenject;

namespace FireSlop.Cats
{
    public sealed class CatView : MonoBehaviour, IAutoSprayTarget
    {
        [SerializeField] private Transform _visual;
        [SerializeField] private Collider[] _bodies;
        [SerializeField] private SphereCollider _airborneTarget;
        [SerializeField] private AudioSource _hiss;
        [SerializeField] private AudioSource _calmAudio;

        [SerializeField] private WaterKnockdownView _knockdown;
        [SerializeField] private CatCarryable _carryable;

        private CatModel _model;
        private Vector3 _homePosition;
        private Quaternion _homeRotation;
        private Pose _travelStart;
        private bool _prepared;

        public bool RequestsAutoSpray => _model != null &&
            (_model.State == CatState.Idle || _model.State == CatState.Warning || _model.State == CatState.Jumping);
        public Vector3 NoticePosition => _bodies[0].bounds.center;
        public event Action<float> WaterReceived;

        [Inject]
        public void Construct(CatModel model)
        {
            Prepare();
            _model = model;
            Subscribe();
        }

        private void Awake()
        {
            Prepare();
        }

        private void Prepare()
        {
            if (_prepared)
            {
                return;
            }

            if (_visual == null || _bodies == null || _bodies.Length == 0 || _hiss == null || _hiss.clip == null ||
                _calmAudio == null || _calmAudio.clip == null)
            {
                throw new InvalidOperationException("Cat View requires its visual, colliders, hiss and calm audio.");
            }

            _homePosition = _visual.localPosition;
            _homeRotation = _visual.localRotation;
            _prepared = true;
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void Subscribe()
        {
            if (_model == null || !isActiveAndEnabled)
            {
                return;
            }

            _model.StateChanged -= RenderState;
            _model.StateChanged += RenderState;
            _model.Calmed -= PlayCalmAudio;
            _model.Calmed += PlayCalmAudio;
            RenderState();
        }

        private void OnDisable()
        {
            if (_model != null)
            {
                _model.StateChanged -= RenderState;
                _model.Calmed -= PlayCalmAudio;
            }

            if (_hiss != null)
            {
                _hiss.Stop();
            }

            if (_calmAudio != null)
            {
                _calmAudio.Stop();
            }
        }

        private void PlayCalmAudio()
        {
            _calmAudio.Play();
        }

        public void FacePlayer(Vector3 position, float degreesPerSecond, float seconds)
        {
            var direction = Vector3.ProjectOnPlane(position - transform.position, Vector3.up);
            if (direction.sqrMagnitude > 0f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation,
                    Quaternion.LookRotation(direction), degreesPerSecond * seconds);
            }
        }

        private void RenderState()
        {
            var state = _model.State;
            _airborneTarget.enabled = state == CatState.Jumping;
            var hasBody = state == CatState.Jumping || state == CatState.Idle || state == CatState.Warning || state == CatState.Returning ||
                state == CatState.Settled || state == CatState.Rescued;
            foreach (var body in _bodies)
            {
                body.enabled = hasBody;
                body.isTrigger = state == CatState.Jumping || state == CatState.Idle || state == CatState.Warning;
            }
            if (state == CatState.Warning)
            {
                _hiss.Play();
            }
            else
            {
                _hiss.Stop();
            }

            if (state == CatState.Jumping)
            {
                _travelStart = new Pose(_visual.position, _visual.rotation);
            }

            if (state == CatState.Settled || state == CatState.Idle)
            {
                _visual.SetLocalPositionAndRotation(_homePosition, _homeRotation);
            }
        }

        public void ApplyWater(in WaterImpact impact)
        {
            var hostile = RequestsAutoSpray;
            if (hostile)
            {
                WaterReceived?.Invoke(impact.Duration);
            }

            if (!_carryable.IsCarried && !_carryable.IsLanding &&
                (_model.State == CatState.Settled || _model.State == CatState.Rescued))
            {
                _knockdown.ReceiveStream(impact, hostile);
            }
        }

        public Pose DetachFromFace()
        {
            var rotation = _visual.rotation * Quaternion.Inverse(_homeRotation);
            var offset = Vector3.Scale(_homePosition, transform.lossyScale);
            var position = _visual.position - rotation * offset;
            transform.SetPositionAndRotation(position, rotation);
            _visual.SetLocalPositionAndRotation(_homePosition, _homeRotation);
            return new Pose(position, rotation);
        }

        public void RenderMotion(Transform eye, CatConfigSO config)
        {
            var state = _model.State;
            if (state != CatState.Jumping && state != CatState.Attached)
            {
                return;
            }

            var target = eye.TransformPoint(config.FaceOffset);
            var rotation = eye.rotation * config.FaceRotation * _homeRotation;
            var progress = state == CatState.Attached ? 1f : Mathf.Clamp01(_model.StateSeconds / config.AttackFlightSeconds);
            var position = Vector3.Lerp(_travelStart.position, target, progress);
            position += Vector3.up * (Mathf.Sin(progress * Mathf.PI) * config.ArcHeight);
            if (state == CatState.Jumping)
            {
                transform.position = position - transform.TransformVector(_homePosition);
            }
            _visual.SetPositionAndRotation(position, Quaternion.Slerp(_travelStart.rotation, rotation, progress));
        }
    }
}
