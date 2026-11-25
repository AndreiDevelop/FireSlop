namespace FireSlop.Interaction
{
    internal sealed class WaterKnockdownModel
    {
        private float _exposure;
        private double _standAt;
        private double _standingAt;
        private double _nextHitAt;

        public bool IsDown { get; private set; }

        public bool ApplyWater(double now, float fallSeconds, float exposure, float threshold)
        {
            if (now < _nextHitAt)
            {
                return false;
            }

            if (!IsDown)
            {
                _exposure += exposure;
                if (_exposure < threshold)
                {
                    return false;
                }
                _exposure = 0f;
            }

            var startsFall = !IsDown;
            IsDown = true;
            _standAt = now + fallSeconds;
            return startsFall;
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
