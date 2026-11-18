using System.Collections;
using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class CabinetView : MonoBehaviour
    {
        [SerializeField] private Transform _leftLeaf;
        [SerializeField] private Transform _rightLeaf;
        [SerializeField] private Vector3 _leftOpenEuler;
        [SerializeField] private Vector3 _rightOpenEuler;
        [SerializeField] private InteractionConfigSO _config;

        private Quaternion _leftClosed;
        private Quaternion _rightClosed;
        private Quaternion _leftOpen;
        private Quaternion _rightOpen;

        private float _openness;

        private void Awake()
        {
            _leftClosed = _leftLeaf.localRotation;
            _rightClosed = _rightLeaf.localRotation;
            _leftOpen = _leftClosed * Quaternion.Euler(_leftOpenEuler);
            _rightOpen = _rightClosed * Quaternion.Euler(_rightOpenEuler);
        }

        internal IEnumerator AnimateSwing(bool isOpen)
        {
            var target = isOpen ? 1f : 0f;
            do
            {
                yield return null;
                SetOpenness(Mathf.MoveTowards(_openness, target, Time.deltaTime / _config.DoorSwingDuration));
            }
            while (_openness != target);
        }

        internal void SetSwingPose(bool isOpen)
        {
            SetOpenness(isOpen ? 1f : 0f);
        }

        private void SetOpenness(float openness)
        {
            _openness = openness;
            var eased = Mathf.SmoothStep(0f, 1f, openness);
            _leftLeaf.localRotation = Quaternion.Slerp(_leftClosed, _leftOpen, eased);
            _rightLeaf.localRotation = Quaternion.Slerp(_rightClosed, _rightOpen, eased);
        }
    }
}
