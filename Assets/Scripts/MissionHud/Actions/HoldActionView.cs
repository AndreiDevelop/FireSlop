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
            Render();
        }

        private void Render()
        {
            if (_label != null)
            {
                _label.text = _model.IsCarryingResident ? "Throw" : "Use tool";
            }
        }

        private void OnDestroy()
        {
            if (_model != null)
            {
                _model.CarryingChanged -= Render;
            }
        }

        private void Start()
        {
            gameObject.SetActive(Application.isMobilePlatform || Touchscreen.current != null);
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
