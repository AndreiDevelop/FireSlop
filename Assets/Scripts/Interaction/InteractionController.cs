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

        [Inject]
        public void Construct(Camera camera, InteractionModel model)
        {
            _camera = camera;
            _model = model;
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
            _model.SetDoorInSight(null);
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
            if (_rescue != null)
            {
                _rescue.SetTarget(target);
            }

            var aimedDoor = FindDoor(target);
            _model.SetDoorInSight(aimedDoor != null ? aimedDoor.Door : null);

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                return;
            }

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
            var target = FindTarget();
            if (_rescue != null)
            {
                _rescue.SetTarget(target);
                if (_rescue.TryInteract())
                {
                    return;
                }
            }

            var door = FindDoor(target);
            if (door == null || door.Door.IsBroken)
            {
                return;
            }

            if (!door.Door.TryToggle())
            {
                _model.ReportClosedDoor();
                return;
            }

            RefreshDoor(door);
        }

        private void RefreshEntrance()
        {
            _model.SetDoorInSight(null);
            _model.SetEntrancePassable(true);
        }

        private void RefreshDoor(DoorView door)
        {
            door.Render(_config.DoorSwingDuration);
            _model.SetDoorInSight(door.Door);
            if (door == _entranceDoor)
            {
                _model.SetEntrancePassable(door.Door.IsPassable);
            }
        }
    }
}
