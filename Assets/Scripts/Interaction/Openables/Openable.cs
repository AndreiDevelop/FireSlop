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
        [SerializeField] private CabinetView _view;
        [SerializeField] private Collider _blockingCollider;

        private OpenableModel _model;
        private Coroutine _swing;

        public bool CanToggle => isActiveAndEnabled && _model != null;
        public bool IsOpen => _model != null && _model.IsOpen;
        public bool IsFullyOpen => IsOpen && _swing == null;

        [Inject]
        internal void Construct(OpenableModel model)
        {
            _model = model;
            if (_view == null || _blockingCollider == null)
            {
                Debug.LogError("Openable requires a swing view and a blocking collider.", this);
                enabled = false;
            }
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

            _blockingCollider.enabled = false;
            _swing = StartCoroutine(SwingTo(IsOpen));
            return true;
        }

        private IEnumerator SwingTo(bool isOpen)
        {
            yield return _view.AnimateSwing(isOpen);
            _blockingCollider.enabled = !isOpen;
            _swing = null;
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
            _blockingCollider.enabled = !IsOpen;
        }
    }
}
