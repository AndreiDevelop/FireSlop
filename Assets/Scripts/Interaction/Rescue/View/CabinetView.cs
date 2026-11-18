using System.Collections;
using UnityEngine;
using Zenject;

namespace FireSlop.Interaction
{
    public sealed class CabinetView : MonoBehaviour
    {
        [SerializeField] private Collider _target;
        [SerializeField] private Transform _leftLeaf;
        [SerializeField] private Transform _rightLeaf;
        [SerializeField] private Vector3 _leftOpenEuler;
        [SerializeField] private Vector3 _rightOpenEuler;
        [SerializeField] private InteractionConfigSO _config;

        private Quaternion _leftClosed;
        private Quaternion _rightClosed;
        private Quaternion _leftOpen;
        private Quaternion _rightOpen;

        private Coroutine _swing;
        private float _openness;

        internal CabinetModel Cabinet { get; private set; }
        public bool IsOpen => Cabinet.IsOpen && _openness >= 1f;

        [Inject]
        internal void Construct(CabinetModel cabinet)
        {
            Cabinet = cabinet;
            _leftClosed = _leftLeaf.localRotation;
            _rightClosed = _rightLeaf.localRotation;
            _leftOpen = _leftClosed * Quaternion.Euler(_leftOpenEuler);
            _rightOpen = _rightClosed * Quaternion.Euler(_rightOpenEuler);
            Cabinet.Changed += Render;
        }

        private void Render()
        {
            if (_swing != null)
            {
                StopCoroutine(_swing);
                _swing = null;
            }

            var target = Cabinet.IsOpen ? 1f : 0f;
            _target.enabled = false;
            if (_openness == target || !isActiveAndEnabled)
            {
                SetOpenness(target);
                _target.enabled = !Cabinet.IsOpen;
                return;
            }

            _swing = StartCoroutine(SwingTo(target));
        }

        private IEnumerator SwingTo(float target)
        {
            while (_openness != target)
            {
                yield return null;
                SetOpenness(Mathf.MoveTowards(_openness, target, Time.deltaTime / _config.DoorSwingDuration));
            }

            _target.enabled = !Cabinet.IsOpen;
            _swing = null;
        }

        private void SetOpenness(float openness)
        {
            _openness = openness;
            var eased = Mathf.SmoothStep(0f, 1f, openness);
            _leftLeaf.localRotation = Quaternion.Slerp(_leftClosed, _leftOpen, eased);
            _rightLeaf.localRotation = Quaternion.Slerp(_rightClosed, _rightOpen, eased);
        }

        private void OnDisable()
        {
            if (_swing == null)
            {
                return;
            }

            StopCoroutine(_swing);
            _swing = null;
            SetOpenness(Cabinet.IsOpen ? 1f : 0f);
            _target.enabled = !Cabinet.IsOpen;
        }

        private void OnDestroy()
        {
            if (Cabinet != null)
            {
                Cabinet.Changed -= Render;
            }
        }
    }
}
