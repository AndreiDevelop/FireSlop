using System.Collections;
using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class DoorView : MonoBehaviour
    {
        [SerializeField] private Transform _leaf;
        [SerializeField] private bool _isLocked = true;
        [SerializeField] private Quaternion _openLocalRotation;

        private Quaternion _closedLocalRotation;
        private Coroutine _swing;
        private float _openness;

        public DoorModel Door { get; private set; }

        private void Awake()
        {
            Door = new DoorModel(_isLocked);
            _closedLocalRotation = _leaf.localRotation;
        }

        private void OnDisable()
        {
            if (_swing == null)
            {
                return;
            }

            StopSwing();
            SetOpenness(Door.IsOpen ? 1f : 0f);
        }

        public bool IsLeafHit(Transform hit)
        {
            return hit.IsChildOf(_leaf);
        }

        public void Render(float swingDuration)
        {
            StopSwing();
            if (Door.IsBroken)
            {
                _leaf.gameObject.SetActive(false);
                return;
            }

            var target = Door.IsOpen ? 1f : 0f;
            if (_openness == target || !isActiveAndEnabled)
            {
                SetOpenness(target);
                return;
            }

            _swing = StartCoroutine(SwingTo(target, swingDuration));
        }

        private IEnumerator SwingTo(float target, float duration)
        {
            while (_openness != target)
            {
                yield return null;
                SetOpenness(Mathf.MoveTowards(_openness, target, Time.deltaTime / duration));
            }

            _swing = null;
        }

        private void SetOpenness(float openness)
        {
            _openness = openness;
            var easedOpenness = Mathf.SmoothStep(0f, 1f, openness);
            _leaf.localRotation = Quaternion.Slerp(_closedLocalRotation, _openLocalRotation, easedOpenness);
        }

        private void StopSwing()
        {
            if (_swing == null)
            {
                return;
            }

            StopCoroutine(_swing);
            _swing = null;
        }
    }
}
