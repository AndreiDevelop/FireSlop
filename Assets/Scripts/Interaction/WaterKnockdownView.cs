using UnityEngine;
using Zenject;

namespace FireSlop.Interaction
{
    public sealed class WaterKnockdownView : MonoBehaviour
    {
        [SerializeField] private WaterKnockdownConfigSO _config;
        [SerializeField] private Animator _animator;
        [SerializeField] private CapsuleCollider _standingCollider;
        [SerializeField] private Rigidbody _standingBody;
        [SerializeField] private ResidentView _resident;
        [SerializeField] private Rigidbody[] _limbs;
        [SerializeField] private Collider[] _limbColliders;

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

        public void ApplyWater(Vector3 direction)
        {
            if (!_model.IsDown && _resident != null && !_resident.CanBeKnockedDown)
            {
                return;
            }

            if (!_model.ApplyWater(Time.timeAsDouble, _config.FallSeconds))
            {
                return;
            }

            _startPosition = transform.position;
            _standingCollider.enabled = false;
            var velocity = direction * _config.KnockbackSpeed;
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

            var standPosition = FindStandPosition(pelvisPosition);
            transform.position = standPosition;
            pelvis.position = pelvisPosition;
            SavePose(_fallenPose);
            _model.BeginStanding(now, _config.StandSeconds, _config.GraceSeconds);
        }

        private Vector3 FindStandPosition(Vector3 pelvisPosition)
        {
            var height = _standingCollider.height;
            var radius = _standingCollider.radius;
            var clearance = _standingCollider.contactOffset;
            var probe = pelvisPosition + Vector3.up * radius;
            if (Physics.Raycast(probe, Vector3.down, out var ground, height * 2f, _config.GroundMask,
                    QueryTriggerInteraction.Ignore) && ground.normal.y > 0.5f)
            {
                var candidate = ground.point + Vector3.up * clearance;
                var center = candidate + transform.TransformVector(_standingCollider.center);
                var axis = Vector3.up * (height * 0.5f - radius);
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

        private void FinishStanding()
        {
            _animator.enabled = true;
            _standingCollider.enabled = true;
            if (_standingBody != null)
            {
                _standingBody.position = transform.position;
                _standingBody.isKinematic = _standingBodyWasKinematic || _resident.IsRescued;
            }

            enabled = false;
        }
    }
}
