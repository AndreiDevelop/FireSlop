using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class HoseConfigSO : ScriptableObject
    {
        [SerializeField, Min(0.01f)] private float _range;
        [SerializeField, Min(0.01f)] private float _force;

        public float Range => _range;
        public float Force => _force;
    }
}
