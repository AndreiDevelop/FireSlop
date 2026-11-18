using System.Collections;
using UnityEngine;

namespace FireSlop.Interaction
{
    [DisallowMultipleComponent]
    public sealed class SwingOpenView : MonoBehaviour
    {
        [SerializeField] private SwingLeaf[] _leaves;
        [SerializeField] private InteractionConfigSO _config;
        [SerializeField] private AudioSource _openingAudio;

        private float _openness;

        private void Awake()
        {
            foreach (var leaf in _leaves)
            {
                leaf.Initialize();
            }
        }

        internal IEnumerator AnimateSwing(bool isOpen)
        {
            var target = isOpen ? 1f : 0f;
            if (isOpen && _openingAudio != null && _openingAudio.clip != null)
            {
                _openingAudio.Play();
            }

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
            foreach (var leaf in _leaves)
            {
                leaf.SetPose(eased);
            }
        }
    }
}
