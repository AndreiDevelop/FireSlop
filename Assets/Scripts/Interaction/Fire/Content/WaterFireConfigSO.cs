using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class WaterFireConfigSO : ScriptableObject
    {
        [SerializeField, Min(0.01f)] private float _secondsToExtinguish;
        [SerializeField, Range(0.01f, 1f)] private float _initialIntensity = 1f;
        [SerializeField, Range(0f, 1f)] private float _passageOpenIntensity = 0.35f;

        public float SecondsToExtinguish => _secondsToExtinguish;
        public float InitialIntensity => _initialIntensity;
        public float PassageOpenIntensity => _passageOpenIntensity;
        public bool IsValid => _secondsToExtinguish > 0f && _initialIntensity > 0f && _initialIntensity <= 1f &&
            _passageOpenIntensity >= 0f && _passageOpenIntensity < _initialIntensity;
    }
}
