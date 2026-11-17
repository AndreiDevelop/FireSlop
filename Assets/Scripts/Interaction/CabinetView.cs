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

        private InteractionModel _model;
        private Quaternion _leftClosed;
        private Quaternion _rightClosed;
        private Quaternion _leftOpen;
        private Quaternion _rightOpen;

        private Coroutine _swing;
        private float _openness;

        public bool IsOpen => _model.CabinetOpened && _openness >= 1f;

        public bool IsHit(Collider target)
        {
            return target != null && target.transform.IsChildOf(transform);
        }

        [Inject]
        public void Construct(InteractionModel model)
        {
            _model = model;
            _leftClosed = _leftLeaf.localRotation;
            _rightClosed = _rightLeaf.localRotation;
            _leftOpen = _leftClosed * Quaternion.Euler(_leftOpenEuler);
            _rightOpen = _rightClosed * Quaternion.Euler(_rightOpenEuler);
            _model.CabinetChanged += Render;
        }

        private void Render()
        {
            if (_swing != null)
            {
                StopCoroutine(_swing);
                _swing = null;
            }

            var target = _model.CabinetOpened ? 1f : 0f;
            _target.enabled = false;
            if (_openness == target || !isActiveAndEnabled)
            {
                SetOpenness(target);
                _target.enabled = !_model.CabinetOpened;
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

            _target.enabled = !_model.CabinetOpened;
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
            SetOpenness(_model.CabinetOpened ? 1f : 0f);
            _target.enabled = !_model.CabinetOpened;
        }

        private void OnDestroy()
        {
            if (_model != null)
            {
                _model.CabinetChanged -= Render;
            }
        }
    }
}
