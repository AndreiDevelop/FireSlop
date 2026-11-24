using UnityEngine;

namespace FireSlop.Interaction
{
    [CreateAssetMenu(menuName = "Fire Slop/Furniture Navigation")]
    public sealed class FurnitureNavigationConfigSO : ScriptableObject
    {
        [SerializeField, Min(0.01f)] private float _moveThreshold;
        [SerializeField, Min(0f)] private float _stationarySeconds;

        public float MoveThreshold => _moveThreshold;
        public float StationarySeconds => _stationarySeconds;
        public bool IsValid => _moveThreshold > 0f && _stationarySeconds >= 0f;
    }
}
