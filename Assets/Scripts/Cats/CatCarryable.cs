using System.Collections;
using FireSlop.Interaction;
using FireSlop.Mission;
using UnityEngine;
using Zenject;

namespace FireSlop.Cats
{
    public sealed class CatCarryable : Carryable, IRescueTarget, IStunEligibility
    {
        [SerializeField] private Collider[] _bodies;

        [SerializeField] private WaterKnockdownView _knockdown;

        public bool IsShowingRecovery => _knockdown.IsShowingRecovery;
        public bool IsKnockedDown => _knockdown.IsReacting;
        private RescueTentView _tent;
        private CatConfigSO _config;

        public bool IsLanding { get; private set; }
        public Transform RescueRoute { get; private set; }

        private CatModel _model;
        private RescueController _carrier;

        public bool CanBeStunned => !IsCarried && _model != null &&
            (_model.State == CatState.Idle || _model.State == CatState.Warning || _model.State == CatState.Settled ||
                _model.State == CatState.Rescued);
        public Vector3 DeliveryPosition => transform.position;
        internal override bool CanPickUp => _model != null && _model.State == CatState.Settled && !IsKnockedDown && base.CanPickUp;

        [Inject]
        public void Construct(CatModel model, RescueController carrier, CatConfigSO config, RescueTentView tent)
        {
            _model = model;
            _tent = tent;
            _config = config;
            _carrier = carrier;
            Body.mass = config.Mass;
        }

        public override ContextActionPrompt GetPrompt(Collider hit)
        {
            foreach (var body in _bodies)
            {
                if (body == hit)
                {
                    return base.GetPrompt(PickupCollider);
                }
            }

            return default;
        }

        protected override void OnAttached(Transform shoulder)
        {
            foreach (var body in _bodies)
            {
                body.enabled = false;
            }
        }

        protected override void OnReleasing()
        {
            foreach (var body in _bodies)
            {
                body.enabled = true;
            }
        }

        internal void PrepareForEncounter()
        {
            if (!Body.isKinematic)
            {
                Body.linearVelocity = Vector3.zero;
                Body.angularVelocity = Vector3.zero;
                Body.isKinematic = true;
            }
        }

        internal void DropFromFace(in Pose pose)
        {
            Body.position = pose.position;
            Body.rotation = pose.rotation;
            Body.isKinematic = false;
            Body.linearVelocity = Vector3.zero;
            Body.angularVelocity = Vector3.zero;
        }

        public bool TryRescue(Vector3 deliveryPosition)
        {
            if (_model == null || IsKnockedDown || !_model.TryRescue())
            {
                return false;
            }

            RescueRoute = _tent.TakeCatRoute();
            var start = RescueRoute.GetChild(0);
            IsLanding = IsCarried;
            var release = new Pose(transform.position, transform.rotation);
            _carrier.Deliver(this, IsLanding ? release.position : start.position);
            Body.isKinematic = true;

            PickupCollider.enabled = true;
            foreach (var body in _bodies)
            {
                body.isTrigger = true;
            }
            if (IsLanding)
            {
                StartCoroutine(JumpToFloor(release, start));
            }
            else
            {
                Body.rotation = start.rotation;
                transform.rotation = start.rotation;
            }
            return true;
        }

        private IEnumerator JumpToFloor(Pose release, Transform landing)
        {
            var travel = landing.position - release.position;
            var forward = Vector3.ProjectOnPlane(travel, Vector3.up);
            var facing = forward.sqrMagnitude > 0.001f ? Quaternion.LookRotation(forward) : landing.rotation;
            var elapsed = 0f;
            while (elapsed < _config.JumpSeconds)
            {
                var progress = elapsed / _config.JumpSeconds;
                var lift = 4f * progress * (1f - progress) * _config.ArcHeight;
                var position = Vector3.Lerp(release.position, landing.position, progress) + Vector3.up * lift;
                transform.SetPositionAndRotation(position, Quaternion.Slerp(release.rotation, facing, progress));
                yield return null;
                elapsed += Time.deltaTime;
            }

            transform.SetPositionAndRotation(landing.position, facing);
            IsLanding = false;
        }
    }
}
