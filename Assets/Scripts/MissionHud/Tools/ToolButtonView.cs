using System;
using FireSlop.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace FireSlop.MissionHud
{
    public sealed class ToolButtonView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private GameObject _shortcutBadge;
        [SerializeField] private TMP_Text _shortcutLabel;
        [SerializeField] private GameObject _selection;

        private string _toolId;
        private Action<string> _select;

        public void Initialize(ToolDefinitionSO tool, Action<string> select)
        {
            if (!_button || !_icon || !_label || !_shortcutBadge || !_shortcutLabel || !_selection)
            {
                Debug.LogError("Tool Button requires its button, icon, label, shortcut badge and selection marker.", this);
                return;
            }

            _toolId = tool.Id;
            _select = select;
            _icon.sprite = tool.Icon;
            _label.text = tool.DisplayName;
            _shortcutBadge.SetActive(tool.SelectionKey != Key.None);
            if (tool.SelectionKey != Key.None)
            {
                _shortcutLabel.text = GetShortcutLabel(tool.SelectionKey);
            }

            _button.onClick.AddListener(SelectTool);
        }

        public void RenderSelection(string selectedId)
        {
            if (_selection)
            {
                _selection.SetActive(selectedId == _toolId);
            }
        }

        private void OnDestroy()
        {
            if (_button)
            {
                _button.onClick.RemoveListener(SelectTool);
            }
        }

        private void SelectTool()
        {
            _select(_toolId);
        }

        private static string GetShortcutLabel(Key key)
        {
            if (Keyboard.current != null)
            {
                return Keyboard.current[key].displayName;
            }

            var label = key.ToString();
            const string digitPrefix = "Digit";
            return label.StartsWith(digitPrefix, StringComparison.Ordinal) ? label.Substring(digitPrefix.Length) : label;
        }
    }
}
