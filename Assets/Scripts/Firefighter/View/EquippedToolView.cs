using System.Collections.Generic;
using FireSlop.Interaction;
using FireSlop.MissionHud;
using UnityEngine;
using Zenject;

namespace FireSlop.Firefighter
{
    public sealed class EquippedToolView : MonoBehaviour
    {
        [SerializeField] private Transform _toolMount;

        private EquippedToolModelView[] _models;
        private ToolSelectionModel _tools;
        private InteractionModel _interaction;
        private bool _isSubscribed;

        [Inject]
        public void Construct(ToolSelectionModel tools, InteractionModel interaction)
        {
            _tools = tools;
            _interaction = interaction;

            if (!_toolMount)
            {
                Debug.LogError("Equipped Tool View requires a tool mount.", this);
                return;
            }

            _models = _toolMount.GetComponentsInChildren<EquippedToolModelView>(true);
            var ids = new HashSet<string>();
            foreach (var model in _models)
            {
                model.Prepare();
                if (string.IsNullOrEmpty(model.ToolId) || !ids.Add(model.ToolId))
                {
                    Debug.LogError("Each equipped model requires a unique tool definition.", model);
                }
            }

            foreach (var tool in tools.Tools)
            {
                if (!ids.Contains(tool.Id))
                {
                    Debug.LogError($"Tool mount has no model for tool '{tool.Id}'.", this);
                }
            }

            if (isActiveAndEnabled)
            {
                Subscribe();
            }
        }

        private void OnEnable()
        {
            if (_models != null)
            {
                Subscribe();
            }
        }

        private void OnDisable()
        {
            if (!_isSubscribed)
            {
                return;
            }

            _tools.SelectionChanged -= Render;
            _tools.QuantityChanged -= OnQuantityChanged;
            _interaction.CarryingChanged -= RenderCurrentSelection;
            _interaction.ContextChanged -= RenderCurrentSelection;
            _isSubscribed = false;
        }

        private void Subscribe()
        {
            if (_isSubscribed)
            {
                return;
            }

            _tools.SelectionChanged += Render;
            _tools.QuantityChanged += OnQuantityChanged;
            _interaction.CarryingChanged += RenderCurrentSelection;
            _interaction.ContextChanged += RenderCurrentSelection;
            _isSubscribed = true;
            RenderCurrentSelection();
        }

        private void OnQuantityChanged(string toolId)
        {
            RenderCurrentSelection();
        }

        private void RenderCurrentSelection()
        {
            Render(_tools.SelectedId);
        }

        private void Render(string selectedId)
        {
            foreach (var model in _models)
            {
                var equipped = !_tools.IsLocked && !_interaction.IsCarrying && model.ToolId == selectedId && _tools.IsAvailable(selectedId);
                model.Render(equipped, _interaction.IsPilotingDrone);
            }
        }
    }
}
