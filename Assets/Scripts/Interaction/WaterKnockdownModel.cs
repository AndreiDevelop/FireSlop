namespace FireSlop.Interaction
{
    internal sealed class WaterKnockdownModel
    {
        private double _standAt;
        private double _standingAt;
        private double _nextHitAt;

        public bool IsDown { get; private set; }

        public bool TryFall(double now, float fallSeconds)
        {
            if (IsDown || now < _nextHitAt)
            {
                return false;
            }

            IsDown = true;
            _standAt = now + fallSeconds;
            return true;
        }

        public bool CanStand(double now)
        {
            return IsDown && now >= _standAt;
        }

        public void BeginStanding(double now, float duration, float graceSeconds)
        {
            IsDown = false;
            _standingAt = now + duration;
            _nextHitAt = _standingAt + graceSeconds;
        }

        public float StandingProgress(double now, float duration)
        {
            return (float)((now - _standingAt) / duration) + 1f;
        }
    }
}
