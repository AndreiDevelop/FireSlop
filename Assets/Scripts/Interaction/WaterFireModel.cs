using System;

namespace FireSlop.Interaction
{
    internal sealed class WaterFireModel
    {
        private readonly float _secondsToExtinguish;
        private float _waterSeconds;

        public WaterFireModel(float secondsToExtinguish)
        {
            _secondsToExtinguish = secondsToExtinguish;
        }

        public float Intensity => 1f - _waterSeconds / _secondsToExtinguish;

        public void ApplyWater(float seconds)
        {
            _waterSeconds = Math.Min(_secondsToExtinguish, _waterSeconds + seconds);
        }
    }
}
