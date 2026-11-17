using System;

namespace FireSlop.Interaction
{
    internal sealed class WaterFireModel
    {
        private readonly float _secondsToExtinguish;
        private readonly float _initialIntensity;
        private readonly float _passageOpenIntensity;
        private float _waterSeconds;

        public WaterFireModel(float secondsToExtinguish, float initialIntensity, float passageOpenIntensity)
        {
            _secondsToExtinguish = secondsToExtinguish;
            _initialIntensity = initialIntensity;
            _passageOpenIntensity = passageOpenIntensity;
        }

        public float Intensity => _initialIntensity * (1f - _waterSeconds / _secondsToExtinguish);
        public bool IsBurning => Intensity > 0f;
        public bool BlocksPassage => Intensity > _passageOpenIntensity;

        public void ApplyWater(float seconds)
        {
            if (!(seconds > 0f))
            {
                return;
            }

            _waterSeconds = Math.Min(_secondsToExtinguish, _waterSeconds + seconds);
        }
    }
}
