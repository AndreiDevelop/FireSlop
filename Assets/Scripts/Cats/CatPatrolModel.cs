using System;

namespace FireSlop.Cats
{
    public sealed class CatPatrolModel
    {
        public int PointIndex { get; private set; }
        public float WaitSeconds { get; private set; }
        public float SafetySeconds { get; private set; }
        public bool IsFireTrapped { get; private set; }

        public void Advance(float seconds)
        {
            if (seconds > 0f && !float.IsInfinity(seconds))
            {
                WaitSeconds = Math.Max(0f, WaitSeconds - seconds);
                SafetySeconds = Math.Max(0f, SafetySeconds - seconds);
            }
        }

        public void NextPoint(int pointCount)
        {
            if (pointCount < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(pointCount));
            }

            PointIndex = (PointIndex + 1) % pointCount;
        }

        public void Wait(float seconds)
        {
            WaitSeconds = Math.Max(0f, seconds);
        }

        public void SetFireSafety(bool isTrapped, float recheckSeconds)
        {
            IsFireTrapped = isTrapped;
            SafetySeconds = Math.Max(0f, recheckSeconds);
        }
    }
}
