using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace FireSlop.MissionHud
{
    public sealed class SettingsButtonView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        private SettingsController _controller;

        [Inject]
        public void Construct(SettingsController controller)
        {
            _controller = controller;
            _button.onClick.AddListener(_controller.Toggle);
        }

        private void OnDestroy()
        {
            if (_controller != null)
            {
                _button.onClick.RemoveListener(_controller.Toggle);
            }
        }
    }
}
