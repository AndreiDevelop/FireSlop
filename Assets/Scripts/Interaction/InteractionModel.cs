using System;
using FireSlop.Firefighter;
using FireSlop.MissionHud;

namespace FireSlop.Interaction
{
    public sealed class InteractionModel : IDisposable
    {
        private readonly ToolSelectionModel _tools;
        private readonly MissionInputModel _input;
        private PrimaryInputSource? _primarySource;
        private bool _enabled = true;
        private bool _doorInSight;

        public InteractionModel(ToolSelectionModel tools, MissionInputModel input)
        {
            _tools = tools;
            _input = input;
            _tools.Changed += CancelPrimary;
            _input.Cancelled += CancelPrimary;
        }

        public bool HoseActive { get; private set; }
        public bool IsEntrancePassable { get; private set; }
        public bool CanInteractWithDoor => _enabled && _doorInSight;
        public bool ShouldCloseDoor { get; private set; }
        public event Action HammerPressed;
        public event Action ContextChanged;
        public event Action ContextRequested;
        public event Action ClosedDoor;

        public void BeginPrimary(PrimaryInputSource source)
        {
            if (!_enabled || _primarySource.HasValue)
            {
                return;
            }

            _primarySource = source;
            if (_tools.Selected == 0)
            {
                HoseActive = true;
            }
            else
            {
                HammerPressed?.Invoke();
            }
        }

        public void EndPrimary(PrimaryInputSource source)
        {
            if (_primarySource == source)
            {
                CancelPrimary();
            }
        }

        private void CancelPrimary()
        {
            _primarySource = null;
            HoseActive = false;
        }

        public void SetActionsEnabled(bool enabled)
        {
            if (_enabled == enabled)
            {
                return;
            }

            _enabled = enabled;
            CancelPrimary();
            ContextChanged?.Invoke();
        }

        public void SetDoorInSight(DoorModel door)
        {
            var visible = door != null && !door.IsBroken;
            var shouldCloseDoor = visible && door.IsOpen;
            if (_doorInSight == visible && ShouldCloseDoor == shouldCloseDoor)
            {
                return;
            }

            _doorInSight = visible;
            ShouldCloseDoor = shouldCloseDoor;
            ContextChanged?.Invoke();
        }

        public void Interact()
        {
            if (CanInteractWithDoor)
            {
                ContextRequested?.Invoke();
            }
        }

        public void ReportClosedDoor()
        {
            ClosedDoor?.Invoke();
        }

        public void SetEntrancePassable(bool isPassable)
        {
            if (IsEntrancePassable == isPassable)
            {
                return;
            }

            IsEntrancePassable = isPassable;
            ContextChanged?.Invoke();
        }

        public void Dispose()
        {
            _tools.Changed -= CancelPrimary;
            _input.Cancelled -= CancelPrimary;
        }
    }
}
