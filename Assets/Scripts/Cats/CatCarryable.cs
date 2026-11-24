using FireSlop.Interaction;
using UnityEngine;
using Zenject;

namespace FireSlop.Cats
{
    public sealed class CatCarryable : Carryable, IRescueTarget, IStunEligibility
    {
        [SerializeField] private Collider[] _bodies;

        private CatModel _model;
        private RescueController _carrier;

        public bool CanBeStunned => !IsCarried && _model != null &&
            (_model.State == CatState.Idle || _model.State == CatState.Warning || _model.State == CatState.Settled);
        public Vector3 DeliveryPosition => transform.position;
        internal override bool CanPickUp => _model != null && _model.State == CatState.Settled && base.CanPickUp;

        [Inject]
        public void Construct(CatModel model, RescueController carrier, CatConfigSO config)
        {
            _model = model;
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
            if (_model == null || !_model.TryRescue())
            {
                return false;
            }

            _carrier.Deliver(this, deliveryPosition);
            return true;
        }
    }
}
