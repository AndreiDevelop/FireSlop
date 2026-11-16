using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class HoseConfigSO : ScriptableObject
    {
        [SerializeField, Min(0.01f)] private float _range;
        [SerializeField, Min(0.01f)] private float _force;
        [SerializeField, Min(0.01f)] private float _launchSpeed;
        [SerializeField, Min(0f)] private float _gravityScale;
        [SerializeField, Min(0.001f)] private float _castRadius;
        [SerializeField, Range(1, 8)] private int _fixedStepsPerSample;
        [SerializeField] private LayerMask _collisionMask;
        [SerializeField] private LayerMask _movingCollisionMask;
        [SerializeField] private LayerMask _pushMask;
        [SerializeField] private LayerMask _fireMask;

        public float Range => _range;
        public float Force => _force;
        public float LaunchSpeed => _launchSpeed;
        public float GravityScale => _gravityScale;
        public float CastRadius => _castRadius;
        public int FixedStepsPerSample => _fixedStepsPerSample;
        public LayerMask CollisionMask => _collisionMask;
        public LayerMask MovingCollisionMask => _movingCollisionMask;
        public LayerMask PushMask => _pushMask;
        public LayerMask FireMask => _fireMask;

        public bool IsValid => _range > 0f && _force > 0f && _launchSpeed > 0f && _gravityScale >= 0f &&
            _castRadius > 0f && _fixedStepsPerSample > 0 && _collisionMask.value != 0 &&
            Includes(_collisionMask, _movingCollisionMask) && Includes(_collisionMask, _fireMask) &&
            Includes(_movingCollisionMask, _pushMask);

        private static bool Includes(LayerMask mask, LayerMask subset)
        {
            return (mask.value & subset.value) == subset.value;
        }
    }
}
