using FireSlop.Mission;
using UnityEngine;
using Zenject;

namespace FireSlop.Interaction
{
    public sealed class ResidentView : Carryable, IRescueTarget, IStunEligibility
    {
        [SerializeField] private Transform _visual;
        [SerializeField] private GameObject _carryPose;
        [SerializeField] private InteractionConfigSO _config;
        [SerializeField] private Vector3 _standingPosition;
        [SerializeField] private Quaternion _standingRotation = Quaternion.identity;

        [SerializeField] private AnimationClip _seatedPose;
        [SerializeField] private Animator _animator;

        private RescueTentView _tent;
        private MissionModel _mission;
        private RescueController _carrier;
        private Openable _cabinet;
        private Rigidbody _occupiedCabinetBody;

        public bool IsRescued => _mission != null && _mission.IsRescued(ContentId);
        public bool CanBeStunned => CanBeKnockedDown && !IsCarried;
        public Vector3 DeliveryPosition => transform.position;
        internal override bool CanPickUp => !IsRescued && base.CanPickUp &&
            (IsFree || (_cabinet != null && _cabinet.IsFullyOpen));
        public bool CanBeKnockedDown => _mission != null && (IsRescued || _mission.IsRevealed(ContentId)) &&
            PickupCollider.enabled && !IsCarried;

        public bool TryRescue(Vector3 deliveryPosition)
        {
            if (_mission == null || (!IsCarried && !PickupCollider.enabled) || !_mission.TryRescue(ContentId))
            {
                return false;
            }

            var seat = _tent.GetSeat(_mission.RescuedCount - 1);
            _carrier.Deliver(this, seat.position);
            transform.rotation = seat.rotation;
            _visual.SetLocalPositionAndRotation(_standingPosition, _standingRotation);
            _animator.enabled = false;
            _seatedPose.SampleAnimation(_visual.gameObject, 0f);
            var hips = _animator.GetBoneTransform(HumanBodyBones.Hips);
            transform.position += seat.position - hips.position;
            Body.position = transform.position;
            Body.rotation = transform.rotation;
            Body.isKinematic = true;
            PickupCollider.enabled = true;
            PickupCollider.isTrigger = true;
            return true;
        }

        [Inject]
        public void Construct(MissionModel mission, RescueController carrier, RescueTentView tent)
        {
            _mission = mission;
            _tent = tent;
            _carrier = carrier;
            Body.mass = _config.ResidentMass;
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

            _visual.gameObject.SetActive(false);
            _carryPose.SetActive(true);
        }

        protected override void OnReleasing()
        {
            _carryPose.SetActive(false);
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
        }
    }
}
