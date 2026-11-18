using System.Collections;
using FireSlop.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace FireSlop.MissionHud
{
    public sealed class ContextActionView : MonoBehaviour
    {
        private const string InteractKeyPrefix = "E  ";

        [SerializeField] private Text _hint;
        [SerializeField] private GameObject _droneControls;
        [SerializeField] private Button _touchButton;
        [SerializeField] private TMP_Text _touchLabel;
        [SerializeField] private Image _touchIcon;
        [SerializeField, Min(0.1f)] private float _closedMessageSeconds = 1.25f;

        private InteractionModel _model;
        private Coroutine _closedMessage;

        private void Awake()
        {
            if (_model == null)
            {
                _touchButton.gameObject.SetActive(false);
            }
        }

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
            var touch = MissionHudPlatform.UsesTouchControls;
            if (!_model.CanInteractWithDoor || _model.HasDroneAction)
            {
                ClearClosed();
            }

            var action = GetActionLabel();
            var hasAction = _model.HasDroneAction || _model.CanInteractWithDoor ||
                            _model.RescueAction != RescueContext.None;
            var showDroneControls = _model.HasDroneAction && _model.IsPilotingDrone && !touch &&
                                    _closedMessage == null;
            _hint.text = InteractKeyPrefix + action;
            if (_model.IsCarryingResident && !_model.IsPilotingDrone && !touch)
            {
                _hint.text += "   |   LMB  Throw";
            }

            if (_closedMessage != null)
            {
                _hint.text = "Closed";
            }

            _hint.gameObject.SetActive(!touch && (_closedMessage != null || (hasAction && !showDroneControls)));
            _droneControls.SetActive(showDroneControls);
            RenderTouchAction(hasAction && touch, action);
        }

        private void RenderTouchAction(bool visible, string action)
        {
            if (visible)
            {
                _touchLabel.text = _closedMessage != null ? "Closed" : action;
                _touchIcon.sprite = _model.ContextIcon;
                _touchIcon.enabled = _touchIcon.sprite != null;
            }

            _touchButton.gameObject.SetActive(visible);
        }

        private string GetActionLabel()
        {
            if (_model.HasDroneAction)
            {
                return _model.IsPilotingDrone ? "Exit flight" : "Use drone";
            }

            if (_model.CanInteractWithDoor)
            {
                return _model.ShouldCloseDoor ? "Close door" : "Open door";
            }

            return _model.RescueAction switch
            {
                RescueContext.OpenCabinet => "Open cabinet",
                RescueContext.CloseCabinet => "Close cabinet",
                RescueContext.PickUp => "Pick up",
                RescueContext.PutDown => "Put down",
                _ => string.Empty
            };
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
