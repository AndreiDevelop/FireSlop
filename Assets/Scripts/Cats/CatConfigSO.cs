using FireSlop.Interaction;
using UnityEngine;

namespace FireSlop.Cats
{
    [CreateAssetMenu(menuName = "Fire Slop/Cat Settings")]
    public sealed class CatConfigSO : ScriptableObject
    {
        [SerializeField, Min(0.1f)] private float _noticeRange;
        [SerializeField, Min(0.1f)] private float _warningSeconds;
        [SerializeField] private WaterKnockdownConfigSO _waterReaction;
        [SerializeField, Min(0.1f)] private float _attackFlightSeconds = 1.2f;
        [SerializeField, Min(0.1f)] private float _jumpSeconds;
        [SerializeField, Min(0f)] private float _arcHeight;
        [SerializeField, Min(0.1f)] private float _mass;
        [SerializeField] private Vector3 _faceOffset;
        [SerializeField] private Vector3 _faceEuler;
        [SerializeField] private LayerMask _obstructionMask;
        [SerializeField, Min(0.1f)] private float _roamSpeed;
        [SerializeField, Min(0.1f)] private float _roamAcceleration;
        [SerializeField, Min(0.1f)] private float _roamTurnSpeed;
        [SerializeField, Min(0.1f)] private float _faceTurnSpeed;
        [SerializeField, Min(0.01f)] private float _roamArrivalDistance;
        [SerializeField, Min(0.1f)] private float _roamPauseSeconds;
        [SerializeField, Min(0.1f)] private float _roamClearance;
        [SerializeField] private LayerMask _roamFireMask;
        [SerializeField, Min(0.01f)] private float _roamObstacleRadius;
        [SerializeField] private LayerMask _roamObstacleMask;

        [SerializeField, Min(2)] private int _removalPresses = 8;
        [SerializeField, Min(0.1f)] private float _removalGapSeconds = 0.45f;

        [SerializeField, Min(0.1f)] private float _removalDrainSeconds = 4f;

        public float RemovalDrainSeconds => _removalDrainSeconds;
        public int RemovalPresses => _removalPresses;
        public float RemovalGapSeconds => _removalGapSeconds;
        public float NoticeRange => _noticeRange;
        public float WarningSeconds => _warningSeconds;
        public float WaterSeconds => _waterReaction.WaterSeconds;
        public float AttackFlightSeconds => _attackFlightSeconds;
        public float JumpSeconds => _jumpSeconds;
        public float ArcHeight => _arcHeight;
        public float Mass => _mass;
        public Vector3 FaceOffset => _faceOffset;
        public Quaternion FaceRotation => Quaternion.Euler(_faceEuler);
        public LayerMask ObstructionMask => _obstructionMask;
        public float RoamSpeed => _roamSpeed;
        public float RoamAcceleration => _roamAcceleration;
        public float RoamTurnSpeed => _roamTurnSpeed;
        public float FaceTurnSpeed => _faceTurnSpeed;
        public float RoamArrivalDistance => _roamArrivalDistance;
        public float RoamPauseSeconds => _roamPauseSeconds;
        public float RoamClearance => _roamClearance;
        public LayerMask RoamFireMask => _roamFireMask;
        public float AttackCooldownSeconds => _waterReaction.CooldownSeconds;
        public float RoamObstacleRadius => _roamObstacleRadius;
        public LayerMask RoamObstacleMask => _roamObstacleMask;
        public bool IsValid => _removalPresses >= 2 && _removalGapSeconds > 0f && _removalDrainSeconds > 0f &&
            _noticeRange > 0f && _warningSeconds > 0f && _waterReaction != null && WaterSeconds > 0f &&
            _attackFlightSeconds > 0f && _jumpSeconds > 0f && _arcHeight >= 0f && _mass > 0f && _faceOffset.z > 0f && _obstructionMask.value != 0 &&
            _roamSpeed > 0f && _roamAcceleration > 0f && _roamTurnSpeed > 0f && _faceTurnSpeed > 0f &&
            _roamArrivalDistance > 0f &&
            _roamPauseSeconds > 0f && _roamClearance > 0f && _roamFireMask.value != 0 &&
            AttackCooldownSeconds > 0f && _roamObstacleRadius > 0f && _roamObstacleMask.value != 0;
    }
}
