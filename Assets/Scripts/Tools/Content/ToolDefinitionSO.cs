using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace FireSlop.Tools
{
    [CreateAssetMenu(menuName = "Fire Slop/Tools/Tool Definition")]
    public sealed class ToolDefinitionSO : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField, Min(0)] private int _index;
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private Key _selectionKey;
        [SerializeField] private ToolAction _action;
        [FormerlySerializedAs("_singleUse")]
        [SerializeField] private bool _consumable;
        [SerializeField, Min(0)] private int _startingQuantity = 1;

        public string Id => _id;
        public int Index => _index;
        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public Key SelectionKey => _selectionKey;
        public bool IsConsumable => _consumable;
        public int StartingQuantity => _startingQuantity;
        public ToolAction Action => _action;
    }
}
