using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class WaterFireConfigSO : ScriptableObject
    {
        [SerializeField, Min(0.01f)] private float _secondsToExtinguish;

        public float SecondsToExtinguish => _secondsToExtinguish;
    }
}
