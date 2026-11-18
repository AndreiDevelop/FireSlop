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
        [SerializeField] private GameObject _desktopHint;

        private InteractionModel _model;
        private MissionInputModel _input;
        private ToolSelectionModel _tools;
        private int? _pointer;

        private MissionHudPlatform _platform;

        [Inject]
        public void Construct(InteractionModel model, MissionInputModel input, ToolSelectionModel tools, MissionHudPlatform platform)
        {
            _platform = platform;
            if (_label == null || _icon == null || _desktopHint == null)
            {
                Debug.LogError("Use Tool requires a label, tool icon, and desktop hint.", this);
                gameObject.SetActive(false);
                return;
            }

            _model = model;
            _platform.Changed += RenderVisibility;
            _input = input;
            _tools = tools;
            _model.CarryingChanged += Render;
            _model.ContextChanged += RenderVisibility;
            _input.Cancelled += ResetPointer;
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
                _platform.Changed -= RenderVisibility;
                _model.ContextChanged -= RenderVisibility;
                _input.Cancelled -= ResetPointer;
                _tools.SelectionChanged -= OnToolChanged;
            }
        }

        private void RenderVisibility()
        {
            var touch = _platform.UsesTouchControls;
            var visible = !_model.IsPilotingDrone;
            _desktopHint.SetActive(visible && !touch);
            gameObject.SetActive(visible && touch);
        }

        public void OnPointerDown(PointerEventData data)
        {
            if (_input == null || !_input.IsEnabled || _pointer.HasValue ||
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
