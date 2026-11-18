using UnityEngine;
using Zenject;

namespace FireSlop.Interaction
{
    public sealed class RescueController : MonoBehaviour
    {
        [SerializeField] private string _cabinetContentId;
        [SerializeField] private ResidentView _resident;
        [SerializeField] private CabinetView _cabinet;
        [SerializeField] private Transform _shoulder;
        [SerializeField] private InteractionConfigSO _config;

        private Camera _camera;
        private InteractionModel _model;
        private Transform _residentParent;
        private CabinetView _aimedCabinet;

        public string CabinetContentId => _cabinetContentId;

        [Inject]
        public void Construct(Camera camera, InteractionModel model)
        {
            _camera = camera;
            _model = model;
            _residentParent = _resident.transform.parent;
            _model.ThrowRequested += Throw;
        }

        public void SetTarget(Collider target)
        {
            var furniture = target != null && target != _resident.Capsule
                ? target.GetComponentInParent<ActionableFurniture>()
                : null;
            _aimedCabinet = furniture != null ? furniture.Openable : null;
            var context = RescueContext.None;
            if (_aimedCabinet != null && _aimedCabinet.Cabinet != null)
            {
                context = _aimedCabinet.Cabinet.IsOpen ? RescueContext.CloseCabinet : RescueContext.OpenCabinet;
            }
            else if (_model.IsCarryingResident)
            {
                context = RescueContext.PutDown;
            }
            else if (target == _resident.Capsule && (_cabinet.IsOpen || _resident.IsFree))
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
                    _resident.Attach(_shoulder);
                    _model.StartCarryingResident();
                    return true;
                case RescueContext.PutDown:
                    TryRelease(false);
                    return true;
                default:
                    return false;
            }
        }

        private bool TryToggleAimedCabinet()
        {
            if (_aimedCabinet == null || _aimedCabinet.Cabinet == null)
            {
                return false;
            }

            var cabinet = _aimedCabinet.Cabinet;
            cabinet.Toggle();
            if (_aimedCabinet == _cabinet && cabinet.IsOpen)
            {
                _model.RevealResident();
            }

            _model.SetRescueContext(cabinet.IsOpen ? RescueContext.CloseCabinet : RescueContext.OpenCabinet);
            return true;
        }

        private void Throw()
        {
            TryRelease(true);
        }

        private void TryRelease(bool throwResident)
        {
            var view = _camera.transform;
            var forward = Vector3.ProjectOnPlane(view.forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.01f)
            {
                forward = _shoulder.parent.forward;
            }

            var rotation = Quaternion.LookRotation(forward);
            var capsule = _resident.Capsule;
            var origin = view.position - Vector3.up * (capsule.height * 0.5f);
            var center = origin + rotation * capsule.center;
            var axis = Vector3.up * (capsule.height * 0.5f - capsule.radius);
            var top = center + axis;
            var bottom = center - axis;
            var distance = _config.ResidentReleaseDistance;
            var offset = forward * distance;
            var mask = _config.AimCollisionMask;
            if (Physics.CapsuleCast(top, bottom, capsule.radius, forward, distance, mask,
                    QueryTriggerInteraction.Ignore) ||
                Physics.CheckCapsule(top + offset, bottom + offset, capsule.radius, mask,
                    QueryTriggerInteraction.Ignore))
            {
                return;
            }

            var velocity = throwResident ? view.forward * _config.ResidentThrowSpeed : Vector3.zero;
            _resident.Release(_residentParent, origin + offset, rotation, velocity);
            _model.StopCarryingResident();
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
