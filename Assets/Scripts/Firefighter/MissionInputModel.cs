using System;
using UnityEngine;

namespace FireSlop.Firefighter
{
    public sealed class MissionInputModel
    {
        private Vector2 _lookDelta;

        public Vector2 Movement { get; private set; }
        public event Action Cancelled;

        public void SetMovement(Vector2 movement)
        {
            Movement = Vector2.ClampMagnitude(movement, 1f);
        }

        public void AddLook(Vector2 screenDelta)
        {
            _lookDelta += screenDelta;
        }

        public Vector2 ConsumeLook()
        {
            var delta = _lookDelta;
            _lookDelta = Vector2.zero;
            return delta;
        }

        public void Cancel()
        {
            Movement = Vector2.zero;
            _lookDelta = Vector2.zero;
            Cancelled?.Invoke();
        }
    }
}
