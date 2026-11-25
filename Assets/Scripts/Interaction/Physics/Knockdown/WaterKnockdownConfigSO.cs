using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class WaterKnockdownConfigSO : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _knockbackSpeed;
        [SerializeField, Min(0f)] private float _tipSpeed;
        [SerializeField, Min(0.01f)] private float _fallSeconds;
        [SerializeField, Min(0.01f)] private float _standSeconds;
        [SerializeField, Min(0.01f)] private float _waterSeconds = 0.3f;
        [SerializeField, Min(0f)] private float _cooldownSeconds = 12f;
        [SerializeField] private LayerMask _groundMask;

        [SerializeField, Min(0f)] private float _liftSpeed = 2.3f;
        [SerializeField, Min(0.1f)] private float _birdSeconds = 2f;

        public float LiftSpeed => _liftSpeed;
        public float BirdSeconds => _birdSeconds;
        public float KnockbackSpeed => _knockbackSpeed;
        public float TipSpeed => _tipSpeed;
        public float FallSeconds => _fallSeconds;
        public float StandSeconds => _standSeconds;
        public float WaterSeconds => _waterSeconds;
        public float CooldownSeconds => _cooldownSeconds;
        public LayerMask GroundMask => _groundMask;
    }
}
