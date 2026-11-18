using FireSlop.Drone;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace FireSlop.Interaction
{
    [DefaultExecutionOrder(100)]
    public sealed class InteractionController : MonoBehaviour
    {
        [SerializeField] private DoorView _entranceDoor;
        [SerializeField] private InteractionConfigSO _config;
        [SerializeField] private RescueController _rescue;

        private Camera _camera;
        private InteractionModel _model;
        private DroneController _drone;

        [Inject]
        public void Construct(Camera camera, InteractionModel model, [InjectOptional] DroneController drone)
        {
            _camera = camera;
            _model = model;
            _drone = drone;
            if (isActiveAndEnabled)
            {
                Subscribe();
            }
        }

        private void OnEnable()
        {
            if (_model != null)
            {
                Subscribe();
            }
        }

        private void OnDisable()
        {
            if (_model == null)
            {
                return;
            }

            _model.ContextRequested -= Interact;
            _entranceDoor.Broken -= RefreshEntrance;
            _entranceDoor.Openable.Toggled -= RefreshEntrance;
            _model.SetAimedPrompt(default);
            _model.EndPrimary(PrimaryInputSource.Mouse);
        }

        private void Subscribe()
        {
            _model.ContextRequested -= Interact;
            _entranceDoor.Broken -= RefreshEntrance;
            _entranceDoor.Openable.Toggled -= RefreshEntrance;
            _model.ContextRequested += Interact;
            _entranceDoor.Broken += RefreshEntrance;
            _entranceDoor.Openable.Toggled += RefreshEntrance;
        }

        private void Update()
        {
            if (_model == null || !Application.isFocused)
            {
                return;
            }

            ResolveTarget();

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
            {
                _model.Interact();
            }

            var mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            if (mouse.leftButton.wasReleasedThisFrame || Cursor.lockState != CursorLockMode.Locked)
            {
                _model.EndPrimary(PrimaryInputSource.Mouse);
                return;
            }

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _model.BeginPrimary(PrimaryInputSource.Mouse);
            }
        }

        private Collider FindTarget()
        {
            var view = _camera.transform;
            if (!Physics.Raycast(view.position, view.forward, out var hit, _config.ContextRange,
                    _config.AimCollisionMask, QueryTriggerInteraction.Ignore))
            {
                return null;
            }

            return hit.collider;
        }

        private IContextActionTarget ResolveTarget()
        {
            var hit = FindTarget();
            var target = hit != null ? hit.GetComponentInParent<IContextActionTarget>() : null;
            var prompt = target != null ? target.GetPrompt(hit) : default;
            _model.SetAimedPrompt(prompt);
            return prompt.IsAvailable ? target : null;
        }

        private void Interact()
        {
            if (_model.HasDroneAction && _drone != null)
            {
                _drone.TogglePilot();
                return;
            }

            var target = ResolveTarget();
            if (_model.ContextPrompt.IsBlocked)
            {
                _model.ReportActionBlocked();
                return;
            }

            if (target != null)
            {
                target.Interact();
            }
            else if (_model.IsCarrying && _rescue != null)
            {
                _rescue.PutDown();
            }

            ResolveTarget();
        }

        private void RefreshEntrance()
        {
            _model.SetEntrancePassable(_entranceDoor.IsPassable);
        }
    }
}
