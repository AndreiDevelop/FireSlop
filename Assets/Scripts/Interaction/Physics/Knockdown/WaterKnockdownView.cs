using UnityEngine;
using Zenject;

namespace FireSlop.Interaction
{
    public sealed class WaterKnockdownView : WaterPushable
    {
        [SerializeField] private WaterKnockdownConfigSO _config;
        [SerializeField] private Animator _animator;
        [SerializeField] private CapsuleCollider _standingCollider;
        [SerializeField] private Rigidbody _standingBody;
        [SerializeField] private ResidentView _resident;
        [SerializeField] private Rigidbody[] _limbs;
        [SerializeField] private Collider[] _limbColliders;

        [SerializeField] private Collider[] _additionalStandingColliders = System.Array.Empty<Collider>();
        [SerializeField] private GameObject _recoveryBirds;

        private bool _animatorWasEnabled;
        private double _hideBirdsAt;
        public bool IsShowingRecovery => !IsReacting && Time.timeAsDouble < _hideBirdsAt;
        public bool IsReacting { get; private set; }

        private WaterKnockdownModel _model;
        private Pose[] _idlePose;
        private Pose[] _fallenPose;
        private Vector3 _startPosition;
        private bool _standingBodyWasKinematic;

        [Inject]
        internal void Construct(WaterKnockdownModel model)
        {
            _model = model;
        }

        private void Awake()
        {
            _idlePose = new Pose[_limbs.Length];
            _fallenPose = new Pose[_limbs.Length];
            enabled = false;
        }

        public override void ApplyWater(in WaterImpact impact)
        {
            if (impact.Impulse <= 0f)
            {
                return;
            }

            ReceiveStream(impact);
        }

        internal void ReceiveStream(in WaterImpact impact, bool thresholdMet = false)
        {
            TryKnockDown(impact.Direction, impact.Duration, thresholdMet);
            var hitBody = impact.Hit.rigidbody;
            if (_model.IsDown && hitBody != null && !hitBody.isKinematic)
            {
                var acceleration = Mathf.Min(impact.Impulse / hitBody.mass, _config.KnockbackSpeed);
                hitBody.linearVelocity = Vector3.ClampMagnitude(hitBody.linearVelocity + impact.Direction * acceleration,
                    _config.KnockbackSpeed * 2f);
            }
        }

        private void TryKnockDown(Vector3 direction, float duration, bool thresholdMet)
        {
            if (!_model.IsDown && _resident != null && !_resident.CanBeKnockedDown)
            {
                return;
            }

            if (!_model.ApplyWater(Time.timeAsDouble, _config.FallSeconds, duration, thresholdMet ? 0f : _config.WaterSeconds))
            {
                return;
            }

            IsReacting = true;
            SetRecoveryBirds(false);
            _animatorWasEnabled = _animator.enabled;
            _startPosition = transform.position;
            _standingCollider.enabled = false;
            foreach (var collider in _additionalStandingColliders)
            {
                collider.enabled = false;
            }

            var velocity = direction * _config.KnockbackSpeed + Vector3.up * _config.LiftSpeed;
            if (_standingBody != null)
            {
                _standingBodyWasKinematic = _standingBody.isKinematic;
                velocity += _standingBody.linearVelocity;
                _standingBody.isKinematic = true;
            }

            _animator.enabled = false;
            SavePose(_idlePose);
            foreach (var limbCollider in _limbColliders)
            {
                limbCollider.enabled = true;
            }

            for (var index = 0; index < _limbColliders.Length; index++)
            {
                for (var other = index + 1; other < _limbColliders.Length; other++)
                {
                    Physics.IgnoreCollision(_limbColliders[index], _limbColliders[other]);
                }
            }

            var pivot = _limbs[0].worldCenterOfMass;
            var spin = Vector3.Cross(Vector3.up, direction) * _config.TipSpeed;
            foreach (var limb in _limbs)
            {
                limb.isKinematic = false;
                limb.linearVelocity = velocity + Vector3.Cross(spin, limb.worldCenterOfMass - pivot);
                limb.angularVelocity = spin;
            }

            enabled = true;
        }

        private void LateUpdate()
        {
            var now = Time.timeAsDouble;
            if (!IsReacting)
            {
                if (now >= _hideBirdsAt || !_standingCollider.enabled)
                {
                    SetRecoveryBirds(false);
                    enabled = false;
                }
                return;
            }

            if (_model.CanStand(now))
            {
                BeginStanding(now);
            }

            if (_model.IsDown)
            {
                return;
            }

            var progress = Mathf.SmoothStep(0f, 1f, _model.StandingProgress(now, _config.StandSeconds));
            ApplyStandingPose(progress);
            if (progress >= 1f)
            {
                FinishStanding();
            }
        }

        private void BeginStanding(double now)
        {
            var pelvis = _limbs[0].transform;
            var pelvisPosition = pelvis.position;
            foreach (var limb in _limbs)
            {
                limb.isKinematic = true;
            }

            foreach (var limbCollider in _limbColliders)
            {
                limbCollider.enabled = false;
            }

            var standPosition = _resident != null && _resident.IsRescued ? _startPosition : FindStandPosition(pelvisPosition);
            transform.position = standPosition;
            pelvis.position = pelvisPosition;
            SavePose(_fallenPose);
            _model.BeginStanding(now, _config.StandSeconds, _config.CooldownSeconds);
        }

        private Vector3 FindStandPosition(Vector3 pelvisPosition)
        {
            var scale = transform.lossyScale;
            var direction = _standingCollider.direction;
            var radiusScale = Mathf.Max(Mathf.Abs(scale[(direction + 1) % 3]), Mathf.Abs(scale[(direction + 2) % 3]));
            var radius = _standingCollider.radius * radiusScale;
            var height = Mathf.Max(2f * radius, _standingCollider.height * Mathf.Abs(scale[direction]));
            var localAxis = Vector3.zero;
            localAxis[direction] = 1f;
            var axis = transform.TransformDirection(localAxis) * (height * 0.5f - radius);
            var clearance = _standingCollider.contactOffset;
            var probe = pelvisPosition + Vector3.up * radius;
            if (Physics.Raycast(probe, Vector3.down, out var ground, height * 2f, _config.GroundMask,
                    QueryTriggerInteraction.Ignore) && ground.normal.y > 0.5f)
            {
                var candidate = ground.point + Vector3.up * clearance;
                var center = candidate + transform.TransformVector(_standingCollider.center);
                if (!Physics.CheckCapsule(center - axis, center + axis, radius, Physics.DefaultRaycastLayers,
                        QueryTriggerInteraction.Ignore))
                {
                    return candidate;
                }
            }

            return _startPosition;
        }

        private void SavePose(Pose[] poses)
        {
            for (var index = 0; index < _limbs.Length; index++)
            {
                var bone = _limbs[index].transform;
                poses[index] = new Pose(bone.localPosition, bone.localRotation);
            }
        }

        private void ApplyStandingPose(float progress)
        {
            for (var index = 0; index < _limbs.Length; index++)
            {
                var fallen = _fallenPose[index];
                var idle = _idlePose[index];
                var position = Vector3.Lerp(fallen.position, idle.position, progress);
                var rotation = Quaternion.Slerp(fallen.rotation, idle.rotation, progress);
                _limbs[index].transform.SetLocalPositionAndRotation(position, rotation);
            }
        }

        private void SetRecoveryBirds(bool visible)
        {
            if (_recoveryBirds != null)
            {
                _recoveryBirds.SetActive(visible);
            }
        }

        private void FinishStanding()
        {
            IsReacting = false;
            _animator.enabled = _animatorWasEnabled;
            foreach (var collider in _additionalStandingColliders)
            {
                collider.enabled = true;
            }
            _standingCollider.enabled = true;
            if (_standingBody != null)
            {
                _standingBody.position = transform.position;
                _standingBody.isKinematic = _standingBodyWasKinematic || (_resident != null && _resident.IsRescued);
            }

            _hideBirdsAt = Time.timeAsDouble + _config.BirdSeconds;
            SetRecoveryBirds(true);
            enabled = _recoveryBirds != null;
        }
    }
}
