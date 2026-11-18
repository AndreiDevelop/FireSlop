using UnityEngine;
using Zenject;

namespace FireSlop.Interaction
{
    public sealed class RescueController : MonoBehaviour
    {
        [SerializeField] private string _cabinetContentId;
        [SerializeField] private ResidentView _resident;
        [SerializeField] private SwingOpenView _cabinet;
        [SerializeField] private Transform _shoulder;
        [SerializeField] private InteractionConfigSO _config;

        private Camera _camera;
        private InteractionModel _model;
        private Carryable _aimedCarryable;
        private Carryable _carried;
        private Openable _rescueCabinet;
        private Openable _aimedCabinet;

        public string CabinetContentId => _cabinetContentId;

        [Inject]
        public void Construct(Camera camera, InteractionModel model)
        {
            _camera = camera;
            _model = model;
            _rescueCabinet = _cabinet != null ? _cabinet.GetComponent<Openable>() : null;
            if (_rescueCabinet == null)
            {
                Debug.LogError("The rescue cabinet requires an Openable component.", this);
            }

            _model.ThrowRequested += Throw;
        }

        public void SetTarget(Collider target)
        {
            _aimedCarryable = target != null ? target.GetComponentInParent<Carryable>() : null;
            if (_aimedCarryable != null && target != _aimedCarryable.PickupCollider)
            {
                _aimedCarryable = null;
            }

            _aimedCabinet = target != null && _aimedCarryable == null
                ? target.GetComponentInParent<Openable>()
                : null;
            var context = RescueContext.None;
            if (_aimedCabinet != null && _aimedCabinet.CanToggle)
            {
                context = _aimedCabinet.IsOpen ? RescueContext.CloseCabinet : RescueContext.OpenCabinet;
            }
            else if (_model.IsCarrying)
            {
                context = RescueContext.PutDown;
            }
            else if (CanPickUpTarget())
            {
                context = RescueContext.PickUp;
            }

            _model.SetRescueContext(context);
        }

        public bool TryInteract()
        {
            switch (_model.RescueAction)
            {
                case RescueContext.OpenCabinet:
                case RescueContext.CloseCabinet:
                    return TryToggleAimedCabinet();
                case RescueContext.PickUp:
                    return TryPickUp();
                case RescueContext.PutDown:
                    return TryRelease(false);
                default:
                    return false;
            }
        }

        private bool TryToggleAimedCabinet()
        {
            var cabinet = _aimedCabinet;
            if (cabinet == null || !cabinet.TryToggle())
            {
                return false;
            }

            if (cabinet == _rescueCabinet && cabinet.IsOpen)
            {
                _model.RevealResident();
            }

            _model.SetRescueContext(cabinet.IsOpen ? RescueContext.CloseCabinet : RescueContext.OpenCabinet);
            return true;
        }

        private bool CanPickUpTarget()
        {
            if (_model.IsCarrying || _aimedCarryable == null || !_aimedCarryable.CanPickUp)
            {
                return false;
            }

            if (_aimedCarryable != _resident)
            {
                return true;
            }

            var cabinetOpen = _rescueCabinet != null && _rescueCabinet.IsFullyOpen;
            return !_resident.IsRescued && (_resident.IsFree || cabinetOpen);
        }

        private bool TryPickUp()
        {
            if (!CanPickUpTarget())
            {
                return false;
            }

            _carried = _aimedCarryable;
            _carried.Attach(_shoulder);
            _model.StartCarrying(_carried.ContentId);
            return true;
        }

        private void Throw()
        {
            TryRelease(true);
        }

        private bool TryRelease(bool throwObject)
        {
            if (_carried == null)
            {
                return false;
            }

            var view = _camera.transform;
            var forward = Vector3.ProjectOnPlane(view.forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.01f)
            {
                forward = _shoulder.parent.forward;
            }

            var velocity = throwObject ? view.forward * _config.ThrowSpeed : Vector3.zero;
            if (!_carried.TryRelease(view.position, forward, _config.ReleaseDistance, _config.AimCollisionMask, velocity))
            {
                return false;
            }

            _carried = null;
            _model.StopCarrying();
            return true;
        }

        private void OnDestroy()
        {
            if (_model != null)
            {
                _model.ThrowRequested -= Throw;
            }
        }
    }
}
