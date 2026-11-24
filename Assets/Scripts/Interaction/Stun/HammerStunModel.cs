namespace FireSlop.Interaction
{
    public sealed class HammerStunModel
    {
        private float _remainingSeconds;

        public bool IsActive => _remainingSeconds > 0f;

        public void Restart(float duration)
        {
            _remainingSeconds = duration;
        }

        public void Advance(float seconds)
        {
            if (IsActive)
            {
                _remainingSeconds -= seconds;
            }
        }

        public void Clear()
        {
            _remainingSeconds = 0f;
        }
    }
}
