using System;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

namespace FireSlop.Orientation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public sealed class MobileOrientationGate : MonoBehaviour
    {
        [SerializeField] private GameObject _overlay;
        [SerializeField, Tooltip("Overrides Device Simulator detection for manual Play Mode testing.")]
        private bool _simulateMobileInEditor;
        [SerializeField] private bool _simulatePortraitInEditor = true;

        private OrientationModel _model;
        private OrientationController _controller;
        private bool _subscribed;
        private int _browserSubscription;

        [Inject]
        public void Construct(OrientationModel model, OrientationController controller)
        {
            if (_overlay == null || _overlay.transform.parent != transform)
            {
                throw new InvalidOperationException("Assign the overlay child on MobileOrientationGate.");
            }

            _model = model;
            _controller = controller;
            Subscribe();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void Subscribe()
        {
            if (_model == null || _subscribed || !isActiveAndEnabled)
            {
                return;
            }

            _subscribed = true;
            _model.Changed += Render;
            _browserSubscription = MobileOrientationPlatform.Subscribe(gameObject.name);
            RefreshVisibility();
            Render();
        }

        private void OnDisable()
        {
            if (!_subscribed)
            {
                return;
            }

            _subscribed = false;
            MobileOrientationPlatform.Unsubscribe(_browserSubscription);
            _model.Changed -= Render;
            _controller.Refresh(false, false);
            Render();
        }

        private void OnRectTransformDimensionsChange()
        {
            RefreshVisibility();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                RefreshVisibility();
            }
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (!isPaused)
            {
                RefreshVisibility();
            }
        }

        [Preserve]
        public void OnBrowserOrientationChanged(string unused)
        {
            RefreshVisibility();
        }

        public void RefreshVisibility()
        {
            if (!_subscribed)
            {
                return;
            }

#if UNITY_EDITOR
            if (_simulateMobileInEditor)
            {
                _controller.Refresh(true, _simulatePortraitInEditor);
                return;
            }
#endif
            _controller.Refresh(MobileOrientationPlatform.IsMobile, MobileOrientationPlatform.IsPortrait);
        }

        private void Render()
        {
            _overlay.SetActive(_model.IsBlocking);
        }
    }
}
