using System;
using FireSlop.Drone;
using FireSlop.Firefighter;
using FireSlop.MissionHud;
using FireSlop.Tools;
using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class InteractionModel : IDisposable
    {
        private readonly ToolSelectionModel _tools;
        private readonly MissionInputModel _input;
        private readonly DroneModel _flight;
        private PrimaryInputSource? _primarySource;
        private bool _autoHoseActive;
        private bool _enabled = true;
        private bool _doorInSight;
        private Sprite _doorIcon;
        private Sprite _droneIcon;

        public InteractionModel(ToolSelectionModel tools, MissionInputModel input, DroneModel flight)
        {
            _tools = tools;
            _input = input;
            _flight = flight;
            _flight.ContextChanged += NotifyContextChanged;
            _tools.SelectionChanged += OnToolSelectionChanged;
            _input.Cancelled += InterruptPrimary;
        }

        private RescueContext _rescueContext;

        public bool HasRevealedResident { get; private set; }
        public string CarriedContentId { get; private set; }
        public bool IsCarrying => CarriedContentId != null;
        public RescueContext RescueAction => _enabled ? _rescueContext : RescueContext.None;
        public event Action ThrowRequested;
        public event Action CabinetRevealed;
        public event Action CarryingChanged;

        public bool HoseActive { get; private set; }
        public bool CanSprayHose => CanUsePrimary && !IsCarrying && _tools.SelectedAction == ToolAction.Hose;
        public bool IsHoseInUseManually => _primarySource.HasValue && CanSprayHose;
        public bool IsEntrancePassable { get; private set; }
        public bool CanInteractWithDoor => _enabled && !_flight.IsPiloted && _doorInSight;
        public bool HasDroneAction => _enabled && (_flight.IsPiloted || _flight.IsOnPlatform);
        public bool IsPilotingDrone => _flight.IsPiloted;
        public bool ShouldCloseDoor { get; private set; }
        public Sprite ContextIcon
        {
            get
            {
                if (HasDroneAction)
                {
                    return _droneIcon;
                }

                return CanInteractWithDoor ? _doorIcon : null;
            }
        }

        public event Action SignalThrowRequested;
        public event Action HammerPressed;
        public event Action HammerInterrupted;
        public event Action HoseStarted;
        public event Action HoseStopped;
        public event Action ContextChanged;
        public event Action ContextRequested;
        public event Action ClosedDoor;

        private bool CanUsePrimary => _enabled && _input.IsEnabled && !_flight.IsPiloted;

        public void BeginPrimary(PrimaryInputSource source)
        {
            if (!CanUsePrimary || _primarySource.HasValue)
            {
                return;
            }

            _primarySource = source;
            if (IsCarrying)
            {
                ThrowRequested?.Invoke();
            }
            else if (_tools.SelectedAction == ToolAction.Hose)
            {
                UpdateHoseState();
            }
            else if (_tools.SelectedAction == ToolAction.Hammer)
            {
                HammerPressed?.Invoke();
            }
            else if (_tools.SelectedAction == ToolAction.SignalCanister && _tools.IsAvailable(_tools.SelectedId))
            {
                SignalThrowRequested?.Invoke();
            }
        }

        public void EndPrimary(PrimaryInputSource source)
        {
            if (_primarySource == source)
            {
                _primarySource = null;
                UpdateHoseState();
            }
        }

        public void SetHoseAim(bool hasReachableFire)
        {
            _autoHoseActive = hasReachableFire && CanSprayHose;
            UpdateHoseState();
        }

        private void UpdateHoseState()
        {
            var active = _autoHoseActive || IsHoseInUseManually;
            if (HoseActive == active)
            {
                return;
            }

            if (active && !_tools.TryCommitUse(ToolAction.Hose))
            {
                return;
            }

            HoseActive = active;
            if (active)
            {
                HoseStarted?.Invoke();
            }
            else
            {
                HoseStopped?.Invoke();
            }
        }

        private void OnToolSelectionChanged(string selectedId)
        {
            InterruptPrimary();
        }

        private void InterruptPrimary()
        {
            _primarySource = null;
            _autoHoseActive = false;
            UpdateHoseState();
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

        public void SetDoorInSight(bool visible, bool shouldCloseDoor = false, Sprite icon = null)
        {
            var doorIcon = visible ? icon : null;
            if (_doorInSight == visible && ShouldCloseDoor == shouldCloseDoor && _doorIcon == doorIcon)
            {
                return;
            }

            _doorInSight = visible;
            ShouldCloseDoor = shouldCloseDoor;
            _doorIcon = doorIcon;
            ContextChanged?.Invoke();
        }

        public void SetDroneIcon(Sprite icon)
        {
            if (_droneIcon == icon)
            {
                return;
            }

            _droneIcon = icon;
            ContextChanged?.Invoke();
        }

        public void Interact()
        {
            if (_input.IsEnabled && (HasDroneAction || CanInteractWithDoor || RescueAction != RescueContext.None))
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

        public void RevealResident()
        {
            if (HasRevealedResident)
            {
                return;
            }

            HasRevealedResident = true;
            CabinetRevealed?.Invoke();
        }

        internal void StartCarrying(string contentId)
        {
            CarriedContentId = contentId;
            InterruptPrimary();
            CarryingChanged?.Invoke();
            SetRescueContext(RescueContext.PutDown);
        }

        internal void StopCarrying()
        {
            CarriedContentId = null;
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
            if (_flight.IsPiloted && (_primarySource.HasValue || _autoHoseActive))
            {
                InterruptPrimary();
            }

            ContextChanged?.Invoke();
        }

        public void Dispose()
        {
            _tools.SelectionChanged -= OnToolSelectionChanged;
            _input.Cancelled -= InterruptPrimary;
            _flight.ContextChanged -= NotifyContextChanged;
        }
    }
}
