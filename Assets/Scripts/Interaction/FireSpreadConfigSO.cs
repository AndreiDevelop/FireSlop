using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class FireSpreadConfigSO : ScriptableObject
    {
        [SerializeField, Min(0.01f)] private float _spreadInterval;
        [SerializeField, Min(0.01f)] private float _reignitionDelay;
        [SerializeField, Min(0.01f)] private float _cellSpacing;
        [SerializeField, Min(0.01f)] private float _corridorLaneSpacing;
        [SerializeField, Min(0.01f)] private float _clearanceRadius;
        [SerializeField, Min(0.01f)] private float _clearanceHeight;
        [SerializeField, Min(1)] private int _ignitionsPerTick;
        [SerializeField] private LayerMask _structureMask;
        [SerializeField] private LayerMask _movableObstructionMask;

        public float SpreadInterval => _spreadInterval;
        public float ReignitionDelay => _reignitionDelay;
        public float CellSpacing => _cellSpacing;
        public float CorridorLaneSpacing => _corridorLaneSpacing;
        public float ClearanceRadius => _clearanceRadius;
        public float ClearanceHeight => _clearanceHeight;
        public int IgnitionsPerTick => _ignitionsPerTick;
        public LayerMask StructureMask => _structureMask;
        public LayerMask MovableObstructionMask => _movableObstructionMask;
        public bool IsValid => _spreadInterval > 0f && _reignitionDelay > 0f && _cellSpacing > 0f &&
            _corridorLaneSpacing > 0f && _corridorLaneSpacing <= _cellSpacing &&
            _clearanceRadius > 0f && _clearanceHeight > _clearanceRadius &&
            _ignitionsPerTick > 0 && _structureMask.value != 0;
    }
}
