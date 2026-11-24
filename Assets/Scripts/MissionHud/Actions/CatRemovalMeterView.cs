using FireSlop.Cats;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace FireSlop.MissionHud
{
    public sealed class CatRemovalMeterView : MonoBehaviour
    {
        [SerializeField] private GameObject _body;
        [SerializeField] private Image _fill;

        private CatFaceModel _face;

        [Inject]
        public void Construct(CatFaceModel face)
        {
            _face = face;
            if (isActiveAndEnabled)
            {
                OnEnable();
            }
        }

        private void OnEnable()
        {
            if (_face != null)
            {
                _face.Changed -= Render;
                _face.ProgressChanged -= Render;
                _face.Changed += Render;
                _face.ProgressChanged += Render;
                Render();
            }
        }

        private void OnDisable()
        {
            if (_face != null)
            {
                _face.Changed -= Render;
                _face.ProgressChanged -= Render;
            }
        }

        private void Render()
        {
            _body.SetActive(_face.IsAttached);
            _fill.fillAmount = _face.Progress;
        }
    }
}
