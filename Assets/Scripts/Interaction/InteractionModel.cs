using System;
using FireSlop.Drone;
using FireSlop.Firefighter;
using FireSlop.MissionHud;

namespace FireSlop.Interaction
{
    public sealed class InteractionModel : IDisposable
    {
        private readonly ToolSelectionModel _tools;
        private readonly MissionInputModel _input;
        private readonly DroneModel _flight;
        private PrimaryInputSource? _primarySource;
        private bool _enabled = true;
        private bool _doorInSight;
        private bool _hasRevealedResident;

        public InteractionModel(ToolSelectionModel tools, MissionInputModel input, DroneModel flight)
        {
            _tools = tools;
            _input = input;
            _flight = flight;
            _flight.ContextChanged += NotifyContextChanged;
            _tools.Changed += InterruptPrimary;
            _input.Cancelled += InterruptPrimary;
        }

        private RescueContext _rescueContext;

        public bool CabinetOpened { get; private set; }
        public bool IsCarryingResident { get; private set; }
        public RescueContext RescueAction => _enabled ? _rescueContext : RescueContext.None;
        public event Action ThrowRequested;
        public event Action CabinetRevealed;
        public event Action CabinetChanged;
        public event Action CarryingChanged;

        public bool HoseActive { get; private set; }
        public bool IsEntrancePassable { get; private set; }
        public bool CanInteractWithDoor => _enabled && !_flight.IsPiloted && _doorInSight;
        public bool HasDroneAction => _enabled && (_flight.IsPiloted || _flight.IsOnPlatform);
        public bool IsPilotingDrone => _flight.IsPiloted;
        public bool ShouldCloseDoor { get; private set; }
        public event Action HammerPressed;
        public event Action HammerInterrupted;
        public event Action HoseStarted;
        public event Action HoseStopped;
        public event Action ContextChanged;
        public event Action ContextRequested;
        public event Action ClosedDoor;

        public void BeginPrimary(PrimaryInputSource source)
        {
            if (!_enabled || _primarySource.HasValue ||
                (_flight.IsPiloted && (IsCarryingResident || _tools.Selected != 0)))
            {
                return;
            }

            _primarySource = source;
            if (IsCarryingResident)
            {
                ThrowRequested?.Invoke();
            }
            else if (_tools.Selected == 0)
            {
                HoseActive = true;
                HoseStarted?.Invoke();
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
            if (!HoseActive)
            {
                return;
            }

            HoseActive = false;
            HoseStopped?.Invoke();
        }

        private void InterruptPrimary()
        {
            CancelPrimary();
            HammerInterrupted?.Invoke();
        }

        public void SetActionsEnabled(bool enabled)
        {
            if (_enabled == enabled)
            {
                return;
            }

            _enabled = enabled;
            InterruptPrimary();
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
            if (HasDroneAction || CanInteractWithDoor || RescueAction != RescueContext.None)
            {
                ContextRequested?.Invoke();
            }
        }

        public void SetRescueContext(RescueContext context)
        {
            if (_rescueContext == context)
            {
                return;
            }

            _rescueContext = context;
            ContextChanged?.Invoke();
        }

        public void ToggleCabinet()
        {
            CabinetOpened = !CabinetOpened;
            if (CabinetOpened && !_hasRevealedResident)
            {
                _hasRevealedResident = true;
                CabinetRevealed?.Invoke();
            }

            CabinetChanged?.Invoke();
        }

        public void StartCarryingResident()
        {
            IsCarryingResident = true;
            InterruptPrimary();
            CarryingChanged?.Invoke();
            SetRescueContext(RescueContext.PutDown);
        }

        public void StopCarryingResident()
        {
            IsCarryingResident = false;
            CarryingChanged?.Invoke();
            SetRescueContext(RescueContext.None);
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

        private void NotifyContextChanged()
        {
            ContextChanged?.Invoke();
        }

        public void Dispose()
        {
            _tools.Changed -= InterruptPrimary;
            _input.Cancelled -= InterruptPrimary;
            _flight.ContextChanged -= NotifyContextChanged;
        }
    }
}
