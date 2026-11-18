using System.Collections.Generic;
using FireSlop.Interaction;
using UnityEngine;
using Zenject;

namespace FireSlop.MissionHud
{
    public sealed class ToolPanelView : MonoBehaviour
    {
        [SerializeField] private ToolButtonView _buttonPrefab;

        private readonly List<ToolButtonView> _buttons = new();
        private ToolSelectionModel _model;
        private InteractionModel _interaction;

        [Inject]
        public void Construct(ToolSelectionModel model, InteractionModel interaction)
        {
            _model = model;
            _interaction = interaction;

            if (!_buttonPrefab)
            {
                Debug.LogError("Tool Panel requires a Tool Button prefab.", this);
                return;
            }

            foreach (var tool in model.Tools)
            {
                var button = Instantiate(_buttonPrefab, transform);
                button.Initialize(tool, model.Select);
                _buttons.Add(button);
            }

            if (isActiveAndEnabled)
            {
                Bind();
            }
        }

        private void OnEnable()
        {
            if (_model != null && _buttonPrefab)
            {
                Bind();
            }
        }

        private void OnDisable()
        {
            if (_model == null)
            {
                return;
            }

            _model.SelectionChanged -= RenderSelection;
            _interaction.ContextChanged -= RenderVisibility;
        }

        private void Bind()
        {
            _model.SelectionChanged += RenderSelection;
            _interaction.ContextChanged += RenderVisibility;
            RenderSelection(_model.SelectedId);
            RenderVisibility();
        }

        private void RenderSelection(string selectedId)
        {
            foreach (var button in _buttons)
            {
                button.RenderSelection(selectedId);
            }
        }

        private void RenderVisibility()
        {
            var visible = !_interaction.IsPilotingDrone;
            foreach (var button in _buttons)
            {
                button.gameObject.SetActive(visible);
            }
        }
    }
}
