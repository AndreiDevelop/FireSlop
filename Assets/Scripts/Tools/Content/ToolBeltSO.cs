using System;
using System.Collections.Generic;
using UnityEngine;

namespace FireSlop.Tools
{
    [CreateAssetMenu(menuName = "Fire Slop/Tools/Tool Belt")]
    public sealed class ToolBeltSO : ScriptableObject
    {
        [SerializeField] private ToolDefinitionSO[] _tools = Array.Empty<ToolDefinitionSO>();
        [SerializeField] private ToolDefinitionSO _defaultTool;

        public IReadOnlyList<ToolDefinitionSO> Tools => Array.AsReadOnly(_tools);
        public ToolDefinitionSO DefaultTool => _defaultTool;
    }
}
