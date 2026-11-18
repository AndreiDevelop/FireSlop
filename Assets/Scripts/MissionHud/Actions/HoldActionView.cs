using FireSlop.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace FireSlop.MissionHud
{
    public sealed class HoldActionView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Image _icon;
        [SerializeField] private GameObject _desktopHint;

        private InteractionModel _model;
        private ToolSelectionModel _tools;
        private int? _pointer;

        [Inject]
        public void Construct(InteractionModel model, ToolSelectionModel tools)
        {
            if (_label == null || _icon == null || _desktopHint == null)
            {
                Debug.LogError("Use Tool requires a label, tool icon, and desktop hint.", this);
                gameObject.SetActive(false);
                return;
            }

            _model = model;
            _tools = tools;
            _model.CarryingChanged += Render;
            _model.ContextChanged += RenderVisibility;
            _tools.SelectionChanged += OnToolChanged;
            Render();
            RenderVisibility();
        }

        private void Render()
        {
            _label.text = _model.IsCarryingResident ? "Throw" : "Use tool";
            _icon.sprite = _tools.SelectedIcon;
            _icon.enabled = !_model.IsCarryingResident;
        }

        private void OnToolChanged(string selectedId)
        {
            Render();
        }

        private void OnDestroy()
        {
            if (_model != null)
            {
                _model.CarryingChanged -= Render;
                _model.ContextChanged -= RenderVisibility;
                _tools.SelectionChanged -= OnToolChanged;
            }
        }

        private void RenderVisibility()
        {
            var touch = MissionHudPlatform.UsesTouchControls;
            var visible = !_model.IsPilotingDrone;
            _desktopHint.SetActive(visible && !touch);
            gameObject.SetActive(visible && touch);
        }

        public void OnPointerDown(PointerEventData data)
        {
            if (_model == null || _pointer.HasValue || data.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            _pointer = data.pointerId;
            _model.BeginPrimary(PrimaryInputSource.Touch);
        }

        public void OnPointerUp(PointerEventData data)
        {
            if (_pointer == data.pointerId)
            {
                ResetPointer();
            }
        }

        private void OnDisable()
        {
            ResetPointer();
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
