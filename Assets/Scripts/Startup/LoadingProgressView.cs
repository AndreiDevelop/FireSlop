using UnityEngine;
using UnityEngine.UI;

namespace FireSlop.Startup
{
    public sealed class LoadingProgressView : MonoBehaviour
    {
        [SerializeField] private Slider _progress;

        public bool IsConfigured => _progress != null;

        public void SetProgress(float progress)
        {
            _progress.SetValueWithoutNotify(progress);
        }
    }
}
