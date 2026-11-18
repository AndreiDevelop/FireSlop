using UnityEngine;
using Zenject;

namespace FireSlop.MissionHud
{
    public sealed class PlatformControlsView : MonoBehaviour
    {
        [SerializeField] private GameObject _body;
        [SerializeField] private bool _touch;

        private MissionHudPlatform _model;

        [Inject]
        public void Construct(MissionHudPlatform platform)
        {
            _model = platform;
            if (isActiveAndEnabled)
            {
                OnEnable();
            }
        }

        private void OnEnable()
        {
            if (_model != null)
            {
                _model.Changed += Render;
                Render();
            }
        }

        private void OnDisable()
        {
            if (_model != null)
            {
                _model.Changed -= Render;
            }
        }

        private void Render()
        {
            _body.SetActive(_model.UsesTouchControls == _touch);
        }
    }
}
