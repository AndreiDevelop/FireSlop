using System.Collections;
using FireSlop.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

namespace FireSlop.MissionHud
{
    public sealed class ContextActionView : MonoBehaviour
    {
        [SerializeField] private Text _hint;
        [SerializeField] private Button _touchButton;
        [SerializeField] private Text _touchLabel;
        [SerializeField] private Text _objective;
        [SerializeField, Min(0.1f)] private float _closedMessageSeconds = 1.25f;

        private InteractionModel _model;
        private Coroutine _closedMessage;

        [Inject]
        public void Construct(InteractionModel model)
        {
            _model = model;
            if (isActiveAndEnabled)
            {
                Subscribe();
            }
        }

        private void OnEnable()
        {
            if (_model != null)
            {
                Subscribe();
            }
        }

        private void OnDisable()
        {
            if (_model == null)
            {
                return;
            }

            _model.ContextChanged -= Render;
            _model.ClosedDoor -= ShowClosed;
            _touchButton.onClick.RemoveListener(_model.Interact);
            ClearClosed();
        }

        private void Subscribe()
        {
            _model.ContextChanged += Render;
            _model.ClosedDoor += ShowClosed;
            _touchButton.onClick.AddListener(_model.Interact);
            Render();
        }

        private void Render()
        {
            var touch = Application.isMobilePlatform || Touchscreen.current != null;
            if (!_model.CanInteractWithDoor)
            {
                ClearClosed();
            }

            var action = _model.RescueAction switch
            {
                RescueContext.OpenCabinet => "Open cabinet",
                RescueContext.PickUp => "Pick up",
                RescueContext.PutDown => "Put down",
                _ => _model.ShouldCloseDoor ? "Close door" : "Open door"
            };
            var hasAction = _model.CanInteractWithDoor || _model.RescueAction != RescueContext.None;
            _hint.text = _closedMessage != null ? "Closed" : "E  " + action;
            if (_model.IsCarryingResident && !touch)
            {
                _hint.text = "E  Put down   |   LMB  Throw";
            }
            _hint.gameObject.SetActive(_closedMessage != null || (hasAction && !touch));
            _touchLabel.text = action;
            _touchButton.gameObject.SetActive(hasAction && touch);
            _objective.text = _model.IsEntrancePassable ? "Enter the building" : "Reach the entrance";
        }

        private void ShowClosed()
        {
            ClearClosed();
            _closedMessage = StartCoroutine(HideClosed());
            Render();
        }

        private IEnumerator HideClosed()
        {
            yield return new WaitForSecondsRealtime(_closedMessageSeconds);
            _closedMessage = null;
            Render();
        }

        private void ClearClosed()
        {
            if (_closedMessage != null)
            {
                StopCoroutine(_closedMessage);
                _closedMessage = null;
            }
        }
    }
}
