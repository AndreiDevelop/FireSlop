using FireSlop.Interaction;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Zenject;

namespace FireSlop.MissionHud
{
    public sealed class HoldActionView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Text _label;

        private InteractionModel _model;
        private int? _pointer;

        [Inject]
        public void Construct(InteractionModel model)
        {
            _model = model;
            _model.CarryingChanged += Render;
            _model.ContextChanged += RenderVisibility;
            Render();
            RenderVisibility();
        }

        private void Render()
        {
            if (_label != null)
            {
                var action = _model.IsCarryingResident ? "Throw" : "Use tool";
                var showMouseHint = !Application.isMobilePlatform && Mouse.current != null;
                _label.text = showMouseHint ? "LMB  " + action : action;
            }
        }

        private void OnDestroy()
        {
            if (_model != null)
            {
                _model.CarryingChanged -= Render;
                _model.ContextChanged -= RenderVisibility;
            }
        }

        private void RenderVisibility()
        {
            var touch = Application.isMobilePlatform || Touchscreen.current != null;
            gameObject.SetActive(touch && !_model.IsPilotingDrone);
        }

        public void OnPointerDown(PointerEventData data)
        {
            if (_pointer.HasValue || data is not ExtendedPointerEventData { pointerType: UIPointerType.Touch })
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
