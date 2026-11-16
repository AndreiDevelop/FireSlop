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

            _model.ContextRequested -= OpenDoor;
            _model.HammerPressed -= BreakDoor;
            _model.SetDoorInSight(false);
            _model.EndPrimary(PrimaryInputSource.Mouse);
        }

        private void Subscribe()
        {
            _model.ContextRequested += OpenDoor;
            _model.HammerPressed += BreakDoor;
        }

        private void Update()
        {
            if (_model == null || !Application.isFocused)
            {
                return;
            }

            var aimedDoor = FindDoor();
            _model.SetDoorInSight(aimedDoor != null && !aimedDoor.Door.IsPassable);

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

        private DoorView FindDoor()
        {
            var view = _camera.transform;
            if (!Physics.Raycast(view.position, view.forward, out var hit, _config.ContextRange,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                return null;
            }

            var door = hit.transform.GetComponentInParent<DoorView>();
            return door != null && door.IsLeafHit(hit.transform) ? door : null;
        }

        private void OpenDoor()
        {
            var door = FindDoor();
            if (door == null || door.Door.IsPassable)
            {
                return;
            }

            if (!door.Door.TryOpen())
            {
                _model.ReportClosedDoor();
                return;
            }

            ShowPassage(door);
        }

        private void BreakDoor()
        {
            var door = FindDoor();
            if (door != null && door.Door.Break())
            {
                ShowPassage(door);
            }
        }

        private void ShowPassage(DoorView door)
        {
            door.Render();
            _model.SetDoorInSight(false);
            if (door == _entranceDoor)
            {
                _model.SetEntrancePassable();
            }
        }
    }
}
