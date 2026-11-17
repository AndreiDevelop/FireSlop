using System;
using UnityEngine;

namespace FireSlop.Firefighter
{
    public sealed class MissionInputModel
    {
        private Vector2 _lookDelta;
        private bool _mouseLookRequested;

        public bool IsEnabled { get; private set; } = true;

        public void SetEnabled(bool enabled)
        {
            IsEnabled = enabled;
            if (!enabled)
            {
                Cancel();
            }
        }

        public Vector2 Movement { get; private set; }
        public event Action Cancelled;

        public void SetMovement(Vector2 movement)
        {
            Movement = IsEnabled ? Vector2.ClampMagnitude(movement, 1f) : Vector2.zero;
        }

        public void AddLook(Vector2 screenDelta)
        {
            if (IsEnabled)
            {
                _lookDelta += screenDelta;
            }
        }

        public Vector2 ConsumeLook()
        {
            var delta = _lookDelta;
            _lookDelta = Vector2.zero;
            return delta;
        }

        public void RequestMouseLook()
        {
            _mouseLookRequested = IsEnabled;
        }

        public bool ConsumeMouseLookRequest()
        {
            var requested = _mouseLookRequested;
            _mouseLookRequested = false;
            return requested;
        }

        public void Cancel()
        {
            Movement = Vector2.zero;
            _lookDelta = Vector2.zero;
            _mouseLookRequested = false;
            Cancelled?.Invoke();
        }
    }
}
