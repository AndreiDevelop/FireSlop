using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace FireSlop.Interaction
{
    public sealed class RescueController : MonoBehaviour
    {
        [SerializeField] private ResidentView[] _residents = Array.Empty<ResidentView>();
        [SerializeField] private Openable[] _cabinets = Array.Empty<Openable>();
        [SerializeField] private Transform _shoulder;
        [SerializeField] private Transform _rescueGrip;
        [SerializeField] private InteractionConfigSO _config;

        private Camera _camera;
        private InteractionModel _model;
        private Carryable _carried;

        public Carryable Carried => _carried;

        public void Deliver(Carryable target, Vector3 position)
        {
            target.Deliver(position);
            if (_carried == target)
            {
                _carried = null;
                _model.StopCarrying();
            }
        }

        [Inject]
        public void Construct(Camera camera, InteractionModel model)
        {
            _camera = camera;
            _model = model;
            _model.ThrowRequested += Throw;
        }

        public void PlaceResidents()
        {
            if (_residents.Length == 0 || _cabinets.Length < _residents.Length)
            {
                throw new InvalidOperationException("Assign a distinct hiding cabinet for every resident.");
            }

            var available = new List<Openable>(_cabinets);
            var unique = new HashSet<Openable>();
            foreach (var cabinet in available)
            {
                if (cabinet == null || !unique.Add(cabinet))
                {
                    throw new InvalidOperationException("Rescue cabinets must be assigned and distinct.");
                }
            }

            foreach (var resident in _residents)
            {
                if (resident == null)
                {
                    throw new InvalidOperationException("Assign all authored rescue residents.");
                }

                var index = Random.Range(0, available.Count);
                resident.PlaceInCabinet(available[index]);
                available.RemoveAt(index);
            }
        }

        internal bool CanPickUp(Carryable target)
        {
            return !_model.IsCarrying && target.CanPickUp;
        }

        internal void TryPickUp(Carryable target)
        {
            if (CanPickUp(target))
            {
                _carried = target;
                var anchor = target.UsesRescueGrip ? _rescueGrip : _shoulder;
                _carried.Attach(anchor);
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
            if (_model != null)
            {
                _model.ThrowRequested -= Throw;
            }
        }
    }
}
