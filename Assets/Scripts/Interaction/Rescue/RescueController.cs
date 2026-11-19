using System;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace FireSlop.Interaction
{
    public sealed class RescueController : MonoBehaviour
    {
        [SerializeField] private string _cabinetContentId;
        [SerializeField] private ResidentView _resident;
        [SerializeField] private SwingOpenView _cabinet;
        [SerializeField] private Openable[] _additionalCabinets = Array.Empty<Openable>();
        [SerializeField] private Transform _shoulder;
        [SerializeField] private InteractionConfigSO _config;

        private Camera _camera;
        private InteractionModel _model;
        private Carryable _carried;
        private Openable _rescueCabinet;
        private Rigidbody _occupiedCabinetBody;

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

        public void PlaceResident(bool useOriginalCabinet)
        {
            if (_rescueCabinet == null)
            {
                throw new InvalidOperationException("Assign the original rescue cabinet before starting the mission.");
            }

            // Index zero keeps the original cabinet eligible on every replay.
            var index = useOriginalCabinet ? 0 : Random.Range(0, _additionalCabinets.Length + 1);
            if (index > 0)
            {
                var cabinet = _additionalCabinets[index - 1];
                if (cabinet == null)
                {
                    throw new InvalidOperationException($"Assign rescue cabinet {index} in the Rescue Controller.");
                }

                _rescueCabinet.Toggled -= OnCabinetToggled;
                _rescueCabinet = cabinet;
                _rescueCabinet.Toggled += OnCabinetToggled;
                _resident.PlaceInCabinet(cabinet.transform);
            }

            // An occupied cabinet must stay aligned with the resident's authored pickup body.
            if (_rescueCabinet.TryGetComponent<Rigidbody>(out var body) && !body.isKinematic)
            {
                body.isKinematic = true;
                _occupiedCabinetBody = body;
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
                if (target == _resident && _occupiedCabinetBody != null)
                {
                    _occupiedCabinetBody.isKinematic = false;
                    _occupiedCabinetBody = null;
                }

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
