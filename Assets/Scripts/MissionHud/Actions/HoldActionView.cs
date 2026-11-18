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
        private int? _pointer;

        [Inject]
        public void Construct(InteractionModel model, MissionInputModel input, ToolSelectionModel tools)
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
            _model.CarryingChanged += Render;
            _input.Cancelled += ResetPointer;
            _tools.SelectionChanged += OnToolChanged;
            Render();
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
                _input.Cancelled -= ResetPointer;
                _tools.SelectionChanged -= OnToolChanged;
            }
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
