using System;
using FireSlop.Drone;
using FireSlop.Firefighter;
using FireSlop.Interaction;
using UnityEngine;
using Zenject;

namespace FireSlop.Cats
{
    public sealed class CatController : MonoBehaviour
    {
        [SerializeField] private CatCarryable _carryable;
        [SerializeField] private CatView _view;

        private CatModel _model;
        private StunModel _stun;
        private CatPatrolModel _patrol;
        private CatFaceModel _face;
        private CatConfigSO _config;
        private MissionInputModel _input;
        private DroneModel _drone;
        private Camera _camera;

        public string ContentId => _carryable.ContentId;

        [Inject]
        public void Construct(CatModel model, CatFaceModel face, CatConfigSO config, MissionInputModel input,
            DroneModel drone, Camera camera, CatPatrolModel patrol, StunModel stun)
        {
            if (_carryable == null || string.IsNullOrWhiteSpace(ContentId) || _view == null)
            {
                throw new InvalidOperationException("Assign a unique Cat content ID and Cat View on each instance.");
            }

            _model = model;
            _stun = stun;
            _face = face;
            _config = config;
            _input = input;
            _drone = drone;
            _camera = camera;
            _patrol = patrol;
            Subscribe();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void Subscribe()
        {
            if (_model == null || !isActiveAndEnabled)
            {
                return;
            }

            _face.RemovalRequested -= Remove;
            _face.RemovalRequested += Remove;
            _view.WaterReceived -= ApplyWater;
            _view.WaterReceived += ApplyWater;
        }

        private void OnDisable()
        {
            if (_model == null)
            {
                return;
            }

            _face.RemovalRequested -= Remove;
            _view.WaterReceived -= ApplyWater;
            _face.Release(ContentId);
            _model.SetThreatened(false);
            _model.Remove();
            _model.FinishReturn();
        }

        private void Update()
        {
            if (_model == null || !_input.IsEnabled || _model.State == CatState.Rescued)
            {
                return;
            }

            if (_stun.IsActive)
            {
                _model.SetThreatened(false);
                return;
            }

            if (_model.State == CatState.Settled)
            {
                if (!_carryable.IsCarried)
                {
                    _model.Advance(Time.deltaTime);
                }

                if (_model.TryEndCooldown(_config.AttackCooldownSeconds, _carryable.IsCarried))
                {
                    _carryable.PrepareForEncounter();
                }
                return;
            }

            _model.Advance(Time.deltaTime);
            if (_model.State == CatState.Idle || _model.State == CatState.Warning)
            {
                _model.SetThreatened(CanReachPlayer());
                if (_model.State == CatState.Warning)
                {
                    _view.FacePlayer(_camera.transform.position, _config.FaceTurnSpeed, Time.deltaTime);
                }

                if (_model.State == CatState.Warning && _model.StateSeconds >= _config.WarningSeconds &&
                    _face.TryReserve(ContentId))
                {
                    _carryable.PrepareForEncounter();
                    _model.TryJump(_config.WarningSeconds);
                }
            }
            else if (_model.State == CatState.Jumping && _model.StateSeconds >= _config.JumpSeconds)
            {
                _model.FinishJump();
                _face.Attach(ContentId, _config);
            }
            else if (_model.State == CatState.Attached)
            {
                _face.ResetIfExpired(Time.timeAsDouble);
            }
            else if (_model.State == CatState.Returning && _model.StateSeconds >= _config.JumpSeconds)
            {
                _model.FinishReturn();
            }
        }

        private bool CanReachPlayer()
        {
            if (_face.IsOccupied || _drone.IsPiloted || _patrol.IsFireTrapped)
            {
                return false;
            }

            var origin = _view.NoticePosition;
            var travel = _camera.transform.position - origin;
            var distance = travel.magnitude;
            return distance <= _config.NoticeRange && !Physics.Raycast(origin, travel.normalized, distance,
                _config.ObstructionMask, QueryTriggerInteraction.Ignore);
        }

        private void ApplyWater(float seconds)
        {
            if (_input.IsEnabled)
            {
                _model.ApplyWater(seconds, _config.WaterSeconds);
            }
        }

        private void Remove()
        {
            if (_face.OccupantId != ContentId || !_face.IsAttached || !_input.IsEnabled)
            {
                return;
            }

            var pose = _view.DetachFromFace();
            _carryable.DropFromFace(pose);
            _model.Remove();
            _face.Release(ContentId);
        }

        private void LateUpdate()
        {
            if (_model != null)
            {
                _view.RenderMotion(_camera.transform, _config);
            }
        }
    }
}
