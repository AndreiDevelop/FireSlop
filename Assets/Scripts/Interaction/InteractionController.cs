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
            _model.SetDroneIcon(_drone != null ? _drone.ActionIcon : null);
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
            _model.SetDoorInSight(false);
            _model.EndPrimary(PrimaryInputSource.Mouse);
        }

        private void Subscribe()
        {
            _model.ContextRequested += Interact;
            _entranceDoor.Broken += RefreshEntrance;
        }

        private void Update()
        {
            if (_model == null || !Application.isFocused)
            {
                return;
            }

            var target = FindTarget();
            var aimedDoor = FindDoor(target);
            if (_rescue != null)
            {
                _rescue.SetTarget(aimedDoor == null ? target : null);
            }

            SetDoorInSight(aimedDoor);

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

        private DoorView FindDoor(Collider target)
        {
            if (target == null)
            {
                return null;
            }

            var door = target.GetComponentInParent<DoorView>();
            return door != null && door.IsLeafHit(target.transform) ? door : null;
        }

        private void Interact()
        {
            if (_model.HasDroneAction && _drone != null)
            {
                _drone.TogglePilot();
                return;
            }

            var target = FindTarget();
            var door = FindDoor(target);
            if (door == null || door.IsBroken)
            {
                if (_rescue != null)
                {
                    _rescue.SetTarget(target);
                    _rescue.TryInteract();
                }

                return;
            }

            if (!door.Openable.TryToggle())
            {
                _model.ReportClosedDoor();
                return;
            }

            SetDoorInSight(door);
            if (door == _entranceDoor)
            {
                _model.SetEntrancePassable(door.IsPassable);
            }
        }

        private void RefreshEntrance()
        {
            _model.SetDoorInSight(false);
            _model.SetEntrancePassable(true);
        }

        private void SetDoorInSight(DoorView door)
        {
            if (door == null || door.IsBroken)
            {
                _model.SetDoorInSight(false);
                return;
            }

            _model.SetDoorInSight(true, door.Openable.IsOpen, door.ActionIcon);
        }
    }
}
