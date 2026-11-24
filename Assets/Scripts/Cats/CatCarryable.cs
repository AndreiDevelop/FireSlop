using FireSlop.Interaction;
using UnityEngine;
using Zenject;

namespace FireSlop.Cats
{
    public sealed class CatCarryable : Carryable, IRescueTarget
    {
        [SerializeField] private Collider[] _bodies;

        private CatModel _model;
        private RescueController _carrier;

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
