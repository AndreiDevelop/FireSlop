using UnityEngine;

namespace FireSlop.Interaction
{
    public readonly struct WaterImpact
    {
        public WaterImpact(RaycastHit hit, Vector3 direction, float duration, float impulse)
        {
            Hit = hit;
            Direction = direction;
            Duration = duration;
            Impulse = impulse;
        }

        public RaycastHit Hit { get; }
        public Vector3 Direction { get; }
        public float Duration { get; }
        public float Impulse { get; }
    }
}
