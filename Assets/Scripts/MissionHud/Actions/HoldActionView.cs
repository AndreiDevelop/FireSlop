using FireSlop.Firefighter;
using FireSlop.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace FireSlop.MissionHud
{
    public sealed class HoldActionView : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Image _icon;

        private InteractionModel _model;
        private MissionInputModel _input;
        private ToolSelectionModel _tools;
        private HudActionCatalogSO _actions;
        private int? _pointer;

        [Inject]
        public void Construct(
            InteractionModel model, MissionInputModel input, ToolSelectionModel tools, HudActionCatalogSO actions)
        {
            if (_label == null || _icon == null)
            {
                Debug.LogError("Use Tool requires a label and tool icon.", this);
                gameObject.SetActive(false);
                return;
            }

            _model = model;
            _input = input;
            _tools = tools;
            _actions = actions;
            if (isActiveAndEnabled)
            {
                OnEnable();
            }
        }

        private void OnEnable()
        {
            if (_model == null)
            {
                return;
            }

            _model.ContextChanged -= Render;
            _input.Cancelled -= ResetPointer;
            _tools.SelectionChanged -= OnToolChanged;
            _model.ContextChanged += Render;
            _input.Cancelled += ResetPointer;
            _tools.SelectionChanged += OnToolChanged;
            Render();
        }

        private void Render()
        {
            var action = _actions.Get(_model.PrimaryActionId);
            var icon = action.Icon;
            if (action.Id == HudActionId.UseTool && icon == null)
            {
                var tool = _tools.GetTool(_tools.SelectedId);
                icon = _tools.IsLocked ? tool.LockedIcon : tool.Icon;
            }

            _label.text = action.Label;
            _icon.sprite = icon;
            _icon.enabled = icon != null;
        }

        private void OnToolChanged(string selectedId)
        {
            Render();
        }

        private void OnDisable()
        {
            if (_model != null)
            {
                _model.ContextChanged -= Render;
                _input.Cancelled -= ResetPointer;
                _tools.SelectionChanged -= OnToolChanged;
                ResetPointer();
            }
        }

        public void OnPointerDown(PointerEventData data)
        {
            if (_input == null || !_input.IsEnabled || _pointer.HasValue ||
                (_tools.IsLocked && !_model.CanThrowCarriedObject) ||
                data.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            _pointer = data.pointerId;
            data.useDragThreshold = false;
            _model.BeginPrimary(PrimaryInputSource.Touch);
        }

        public void OnDrag(PointerEventData data)
        {
            if (_pointer == data.pointerId)
            {
                _input.AddLook(data.delta);
            }
        }

        public void OnPointerUp(PointerEventData data)
        {
            if (_pointer == data.pointerId && data.button == PointerEventData.InputButton.Left)
            {
                ResetPointer();
            }
        }

        private void ResetPointer()
        {
            if (_pointer.HasValue)
            {
                _pointer = null;
                _model.EndPrimary(PrimaryInputSource.Touch);
            }
        }
    }
}
