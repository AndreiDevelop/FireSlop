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
        private Carryable _carried;
        private Openable _rescueCabinet;

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

            if (_rescueCabinet != null)
            {
                _rescueCabinet.Toggled += OnCabinetToggled;
            }

            _model.ThrowRequested += Throw;
        }

        private void OnCabinetToggled()
        {
            if (_rescueCabinet.IsOpen)
            {
                _model.RevealResident();
            }
        }

        internal bool CanPickUp(Carryable target)
        {
            if (_model.IsCarrying || !target.CanPickUp)
            {
                return false;
            }

            if (target != _resident)
            {
                return true;
            }

            var cabinetOpen = _rescueCabinet != null && _rescueCabinet.IsFullyOpen;
            return !_resident.IsRescued && (_resident.IsFree || cabinetOpen);
        }

        internal void TryPickUp(Carryable target)
        {
            if (CanPickUp(target))
            {
                _carried = target;
                _carried.Attach(_shoulder);
                _model.StartCarrying(_carried.ContentId);
            }
        }

        internal void PutDown()
        {
            TryRelease(false);
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
            if (_rescueCabinet != null)
            {
                _rescueCabinet.Toggled -= OnCabinetToggled;
            }

            if (_model != null)
            {
                _model.ThrowRequested -= Throw;
            }
        }
    }
}
