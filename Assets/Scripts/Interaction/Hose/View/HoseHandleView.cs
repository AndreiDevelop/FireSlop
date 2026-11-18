using System.Collections;
using UnityEngine;
using Zenject;

namespace FireSlop.Interaction
{
    public sealed class HoseHandleView : MonoBehaviour
    {
        [SerializeField] private AnimationClip _opening;

        private InteractionModel _interaction;
        private Coroutine _animation;
        private float _time;

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
            if (_opening == null || _opening.length <= 0f)
            {
                Debug.LogError("Hose handle requires an opening animation clip with a positive duration.", this);
                enabled = false;
            }
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

            _time = 0f;
            if (_opening != null)
            {
                _opening.SampleAnimation(gameObject, _time);
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
            }

            _animation = StartCoroutine(MoveHandle());
        }

        private IEnumerator MoveHandle()
        {
            var target = _interaction.HoseActive ? _opening.length : 0f;
            while (!Mathf.Approximately(_time, target))
            {
                _time = Mathf.MoveTowards(_time, target, Time.deltaTime);
                _opening.SampleAnimation(gameObject, _time);
                yield return null;
            }

            _animation = null;
        }
    }
}
