using System;

namespace FireSlop.Interaction
{
    internal sealed class WaterFireModel
    {
        private readonly float _secondsToExtinguish;
        private readonly float _initialIntensity;
        private readonly float _passageOpenIntensity;
        private float _waterSeconds;
        private double _spreadAt;
        private double _reigniteAt;

        public WaterFireModel(float secondsToExtinguish, float initialIntensity, float passageOpenIntensity,
            bool startsBurning = true)
        {
            _secondsToExtinguish = secondsToExtinguish;
            _initialIntensity = initialIntensity;
            _passageOpenIntensity = passageOpenIntensity;
            _waterSeconds = startsBurning ? 0f : secondsToExtinguish;
        }

        public float Intensity => _initialIntensity * (1f - _waterSeconds / _secondsToExtinguish);
        public bool IsBurning => Intensity > 0f;
        public bool BlocksPassage => Intensity > _passageOpenIntensity;
        public event Action Extinguished;

        public WaterFireModel CreateUnlit()
        {
            return new WaterFireModel(_secondsToExtinguish, _initialIntensity, _passageOpenIntensity, false);
        }

        public bool CanSpread(double time)
        {
            return IsBurning && time >= _spreadAt;
        }

        public bool CanIgnite(double time)
        {
            return !IsBurning && time >= _reigniteAt;
        }

        public void Ignite(double time, float spreadInterval)
        {
            _waterSeconds = 0f;
            _spreadAt = time + spreadInterval;
        }

        public void ApplyWater(float seconds, double time, float reignitionDelay)
        {
            if (!IsBurning || !(seconds > 0f))
            {
                return;
            }

            _waterSeconds = Math.Min(_secondsToExtinguish, _waterSeconds + seconds);
            if (!IsBurning)
            {
                _reigniteAt = time + reignitionDelay;
                Extinguished?.Invoke();
            }
        }
    }
}
