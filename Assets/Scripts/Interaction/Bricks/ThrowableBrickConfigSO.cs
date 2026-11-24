using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class ThrowableBrickConfigSO : ScriptableObject
    {
        [SerializeField, Min(0.1f)] private float _mass = 2f;
        [SerializeField, Min(0.1f)] private float _breakSpeed = 2f;
        [SerializeField, Min(1f)] private float _recoveryDepth = 10f;

        public float Mass => _mass;
        public float BreakSpeed => _breakSpeed;
        public float RecoveryDepth => _recoveryDepth;
        public bool IsValid => _mass > 0f && _breakSpeed > 0f &&
            _recoveryDepth >= 1f;
    }
}
