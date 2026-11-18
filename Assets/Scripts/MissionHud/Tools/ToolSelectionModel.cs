using System;
using System.Collections.Generic;
using FireSlop.Tools;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FireSlop.MissionHud
{
    public sealed class ToolSelectionModel
    {
        private readonly Dictionary<string, ToolDefinitionSO> _toolsById = new(StringComparer.Ordinal);
        private ToolDefinitionSO _selected;
        private bool _enabled = true;

        public IReadOnlyList<ToolDefinitionSO> Tools { get; }
        public string SelectedId => _selected.Id;
        public ToolAction SelectedAction => _selected.Action;
        public Sprite SelectedIcon => _selected.Icon;
        public event Action<string> SelectionChanged;

        public ToolSelectionModel(ToolBeltSO belt)
        {
            if (!belt)
            {
                throw new ArgumentException("Assign a Tool Belt asset in Mission Installer.", nameof(belt));
            }

            var tools = new List<ToolDefinitionSO>(belt.Tools.Count);
            var indices = new HashSet<int>();
            var keys = new HashSet<Key>();
            foreach (var tool in belt.Tools)
            {
                if (!tool || string.IsNullOrWhiteSpace(tool.Id) || string.IsNullOrWhiteSpace(tool.DisplayName) ||
                    !tool.Icon || tool.Index < 0 || !Enum.IsDefined(typeof(ToolAction), tool.Action) ||
                    !Enum.IsDefined(typeof(Key), tool.SelectionKey))
                {
                    throw new InvalidOperationException($"Tool Belt '{belt.name}' has an invalid tool: '{tool}'.");
                }

                if (!_toolsById.TryAdd(tool.Id, tool) || !indices.Add(tool.Index) ||
                    (tool.SelectionKey != Key.None && !keys.Add(tool.SelectionKey)))
                {
                    throw new InvalidOperationException(
                        $"Tool Belt '{belt.name}' repeats an ID, index or selection key on '{tool.name}'.");
                }

                tools.Add(tool);
            }

            if (!belt.DefaultTool || !tools.Contains(belt.DefaultTool))
            {
                throw new InvalidOperationException($"Tool Belt '{belt.name}' must select a default tool from its list.");
            }

            tools.Sort(CompareIndex);
            Tools = tools.AsReadOnly();
            _selected = belt.DefaultTool;
        }

        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;
        }

        public void Select(string id)
        {
            if (!_enabled || string.IsNullOrEmpty(id) || id == SelectedId ||
                !_toolsById.TryGetValue(id, out var tool))
            {
                return;
            }

            _selected = tool;
            SelectionChanged?.Invoke(id);
        }

        private static int CompareIndex(ToolDefinitionSO first, ToolDefinitionSO second)
        {
            return first.Index.CompareTo(second.Index);
        }
    }
}
