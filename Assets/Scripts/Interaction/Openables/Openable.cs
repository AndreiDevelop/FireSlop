using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace FireSlop.Interaction
{
    [DisallowMultipleComponent]
    public sealed class Openable : MonoBehaviour
    {
        [FormerlySerializedAs("_openable")]
        [SerializeField] private SwingOpenView _view;
        [SerializeField] private Collider _blockingCollider;

        private OpenableModel _model;
        private Coroutine _swing;

        public bool CanToggle => isActiveAndEnabled && _model != null && !_model.IsLocked;
        public bool IsOpen => _model != null && _model.IsOpen;
        public bool IsFullyOpen => IsOpen && _swing == null;

        [Inject]
        internal void Construct(OpenableModel model)
        {
            _model = model;
            if (_view == null)
            {
                Debug.LogError("Openable requires a swing view.", this);
                enabled = false;
            }
        }

        internal void Lock()
        {
            _model.Lock();
        }

        public bool TryToggle()
        {
            if (!CanToggle)
            {
                return false;
            }

            _model.Toggle();
            if (_swing != null)
            {
                StopCoroutine(_swing);
            }

            SetBlocking(false);
            _swing = StartCoroutine(SwingTo(IsOpen));
            return true;
        }

        private IEnumerator SwingTo(bool isOpen)
        {
            yield return _view.AnimateSwing(isOpen);
            SetBlocking(!isOpen);
            _swing = null;
        }

        private void SetBlocking(bool blocking)
        {
            if (_blockingCollider != null)
            {
                _blockingCollider.enabled = blocking;
            }
        }

        private void OnDisable()
        {
            if (_swing == null)
            {
                return;
            }

            StopCoroutine(_swing);
            _swing = null;
            _view.SetSwingPose(IsOpen);
            SetBlocking(!IsOpen);
        }
    }
}
