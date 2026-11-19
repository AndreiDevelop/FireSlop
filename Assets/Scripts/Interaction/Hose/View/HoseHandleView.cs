using System.Collections;
using UnityEngine;
using Zenject;

namespace FireSlop.Interaction
{
    public sealed class HoseHandleView : MonoBehaviour
    {
        private static readonly int OpeningParameter = Animator.StringToHash("Opening");

        [SerializeField] private Animator _animator;

        private InteractionModel _interaction;
        private Coroutine _animation;
        private float _openingDuration;
        private float _opening;

        [Inject]
        public void Construct(InteractionModel interaction)
        {
            _interaction = interaction;
            if (isActiveAndEnabled)
            {
                Subscribe();
            }
        }

        private void Awake()
        {
            var controller = _animator != null ? _animator.runtimeAnimatorController : null;
            var clips = controller != null ? controller.animationClips : null;
            if (clips == null || clips.Length != 1 || clips[0].length <= 0f)
            {
                Debug.LogError("Hose handle requires an Animator with one opening clip of positive duration.", this);
                enabled = false;
                return;
            }

            _openingDuration = clips[0].length;
        }

        private void OnEnable()
        {
            if (_interaction != null)
            {
                Subscribe();
            }
        }

        private void OnDisable()
        {
            if (_interaction != null)
            {
                _interaction.HoseStarted -= Animate;
                _interaction.HoseStopped -= Animate;
            }

            if (_animation != null)
            {
                StopCoroutine(_animation);
                _animation = null;
            }

            _opening = 0f;
            if (_animator != null && _animator.isActiveAndEnabled)
            {
                _animator.SetFloat(OpeningParameter, _opening);
            }
        }

        private void Subscribe()
        {
            _interaction.HoseStarted += Animate;
            _interaction.HoseStopped += Animate;
            Animate();
        }

        private void Animate()
        {
            if (_animation != null)
            {
                StopCoroutine(_animation);
                _animation = null;
            }

            var target = _interaction.HoseActive ? 1f : 0f;
            if (_opening != target)
            {
                _animation = StartCoroutine(MoveHandle(target));
            }
        }

        private IEnumerator MoveHandle(float target)
        {
            while (_opening != target)
            {
                yield return null;
                _opening = Mathf.MoveTowards(_opening, target, Time.deltaTime / _openingDuration);
                _animator.SetFloat(OpeningParameter, _opening);
            }

            _animation = null;
        }
    }
}
