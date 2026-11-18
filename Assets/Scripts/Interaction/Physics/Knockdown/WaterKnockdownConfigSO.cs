using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class WaterKnockdownConfigSO : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _knockbackSpeed;
        [SerializeField, Min(0f)] private float _tipSpeed;
        [SerializeField, Min(0.01f)] private float _fallSeconds;
        [SerializeField, Min(0.01f)] private float _standSeconds;
        [SerializeField, Min(0f)] private float _graceSeconds;
        [SerializeField] private LayerMask _groundMask;

        public float KnockbackSpeed => _knockbackSpeed;
        public float TipSpeed => _tipSpeed;
        public float FallSeconds => _fallSeconds;
        public float StandSeconds => _standSeconds;
        public float GraceSeconds => _graceSeconds;
        public LayerMask GroundMask => _groundMask;
    }
}
