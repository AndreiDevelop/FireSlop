using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class InteractionConfigSO : ScriptableObject
    {
        [SerializeField, Min(0.1f)] private float _contextRange = 2.5f;
        [SerializeField, Min(0.01f)] private float _doorSwingDuration = 0.4f;
        [SerializeField, Min(0.1f)] private float _hammerReach = 2.5f;
        [SerializeField, Min(0f)] private float _hammerImpactTime = 0.15f;
        [SerializeField, Min(1)] private int _doorHitsToBreak = 3;
        [SerializeField, Min(0.01f)] private float _debrisSettleSeconds = 0.62f;
        [SerializeField, Min(0f)] private float _debrisTravel = 0.68f;
        [SerializeField] private LayerMask _aimCollisionMask;

        public float ContextRange => _contextRange;
        public float DoorSwingDuration => _doorSwingDuration;
        public float HammerReach => _hammerReach;
        public float HammerImpactTime => _hammerImpactTime;
        public int DoorHitsToBreak => _doorHitsToBreak;
        public float DebrisSettleSeconds => _debrisSettleSeconds;
        public float DebrisTravel => _debrisTravel;
        public LayerMask AimCollisionMask => _aimCollisionMask;
    }
}
