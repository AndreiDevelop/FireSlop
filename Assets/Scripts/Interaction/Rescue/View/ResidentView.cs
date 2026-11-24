using FireSlop.Mission;
using UnityEngine;
using Zenject;

namespace FireSlop.Interaction
{
    public sealed class ResidentView : Carryable, IRescueTarget
    {
        [SerializeField] private Transform _visual;
        [SerializeField] private ResidentCarryRagdoll _carryRagdoll;
        [SerializeField] private InteractionConfigSO _config;
        [SerializeField] private Vector3 _standingPosition;
        [SerializeField] private Quaternion _standingRotation = Quaternion.identity;

        private MissionModel _mission;
        private RescueController _carrier;
        private Openable _cabinet;
        private Rigidbody _occupiedCabinetBody;

        public bool IsRescued => _mission != null && _mission.IsRescued(ContentId);
        public Vector3 DeliveryPosition => transform.position;
        internal override bool CanPickUp => !IsRescued && base.CanPickUp &&
            (IsFree || (_cabinet != null && _cabinet.IsFullyOpen));
        public bool CanBeKnockedDown => _mission != null && _mission.IsRevealed(ContentId) &&
            PickupCollider.enabled && !IsRescued;

        public bool TryRescue(Vector3 deliveryPosition)
        {
            if (_mission == null || !_mission.TryRescue(ContentId))
            {
                return false;
            }

            _carrier.Deliver(this, deliveryPosition);
            Body.isKinematic = true;
            PickupCollider.enabled = false;
            return true;
        }

        [Inject]
        public void Construct(MissionModel mission, RescueController carrier)
        {
            _mission = mission;
            _carrier = carrier;
            Body.mass = _config.ResidentMass;
            _carryRagdoll.Initialize();
        }

        internal void PlaceInCabinet(Openable cabinet)
        {
            if (_cabinet != null)
            {
                _cabinet.Toggled -= OnCabinetToggled;
            }

            _cabinet = cabinet;
            _cabinet.Toggled += OnCabinetToggled;
            transform.SetPositionAndRotation(cabinet.transform.position, cabinet.transform.rotation);
            Body.position = transform.position;
            Body.rotation = transform.rotation;
            if (cabinet.TryGetComponent<Rigidbody>(out var body) && !body.isKinematic)
            {
                body.isKinematic = true;
                _occupiedCabinetBody = body;
            }

            OnCabinetToggled();
        }

        protected override void OnAttached(Transform shoulder)
        {
            if (_occupiedCabinetBody != null)
            {
                _occupiedCabinetBody.isKinematic = false;
                _occupiedCabinetBody = null;
            }

            _carryRagdoll.HangFrom(shoulder);
            _visual.gameObject.SetActive(false);
        }

        protected override void OnReleasing()
        {
            _carryRagdoll.Stow();
            _visual.gameObject.SetActive(true);
        }

        private void OnCabinetToggled()
        {
            if (!_cabinet.IsOpen || _mission.IsRevealed(ContentId))
            {
                return;
            }

            _mission.RevealResident(ContentId);
            _visual.SetLocalPositionAndRotation(_standingPosition, _standingRotation);
        }

        private void OnDestroy()
        {
            if (_cabinet != null)
            {
                _cabinet.Toggled -= OnCabinetToggled;
            }

            if (_carryRagdoll != null && _carryRagdoll.transform.parent == null)
            {
                Destroy(_carryRagdoll.gameObject);
            }
        }
    }
}
