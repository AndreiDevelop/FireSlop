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

        public bool IsOpen { get; private set; }
        public Collider Target => _target;

        [Inject]
        public void Construct(InteractionModel model)
        {
            _model = model;
            _leftClosed = _leftLeaf.localRotation;
            _rightClosed = _rightLeaf.localRotation;
            _leftOpen = _leftClosed * Quaternion.Euler(_leftOpenEuler);
            _rightOpen = _rightClosed * Quaternion.Euler(_rightOpenEuler);
            _model.CabinetRevealed += Open;
        }

        private void Open()
        {
            _target.enabled = false;
            StartCoroutine(SwingOpen());
        }

        private IEnumerator SwingOpen()
        {
            var openness = 0f;
            while (openness < 1f)
            {
                yield return null;
                openness = Mathf.MoveTowards(openness, 1f, Time.deltaTime / _config.DoorSwingDuration);
                var eased = Mathf.SmoothStep(0f, 1f, openness);
                _leftLeaf.localRotation = Quaternion.Slerp(_leftClosed, _leftOpen, eased);
                _rightLeaf.localRotation = Quaternion.Slerp(_rightClosed, _rightOpen, eased);
            }

            IsOpen = true;
        }

        private void OnDestroy()
        {
            if (_model != null)
            {
                _model.CabinetRevealed -= Open;
            }
        }
    }
}
