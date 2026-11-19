using System;
using UnityEngine;

namespace FireSlop.Firefighter
{
    public sealed class HammerSwingView : MonoBehaviour
    {
        private static readonly int RestState = Animator.StringToHash("Base Layer.Rest");
        private static readonly int SwingState = Animator.StringToHash("Base Layer.Swing");

        [SerializeField] private Animator _animator;

        private bool _impactPending;

        public bool IsSwinging { get; private set; }
        public bool CanSwing => isActiveAndEnabled && _animator != null && _animator.isActiveAndEnabled &&
            _animator.runtimeAnimatorController != null && !IsSwinging;

        public event Action Impact;

        public void PlaySwing()
        {
            if (!CanSwing)
            {
                return;
            }

            IsSwinging = true;
            _impactPending = true;
            _animator.Play(SwingState, 0, 0f);
        }

        public void StopSwing()
        {
            if (!IsSwinging)
            {
                return;
            }

            ClearSwing();
            if (_animator.isActiveAndEnabled)
            {
                _animator.Play(RestState, 0, 0f);
            }
        }

        public void OnSwingImpact()
        {
            if (!_impactPending || !isActiveAndEnabled)
            {
                return;
            }

            _impactPending = false;
            Impact?.Invoke();
        }

        public void OnSwingFinished()
        {
            StopSwing();
        }

        private void OnDisable()
        {
            ClearSwing();
        }

        private void ClearSwing()
        {
            IsSwinging = false;
            _impactPending = false;
        }
    }
}
