using System;
using UnityEngine;
using Zenject;

namespace FireSlop.Cats
{
    public sealed class CatWarningView : MonoBehaviour
    {
        [SerializeField] private GameObject _body;

        private CatModel _model;
        private Transform _eye;

        public bool IsVisible => _body.activeSelf;

        [Inject]
        public void Construct(CatModel model, Camera camera)
        {
            if (_body == null)
            {
                throw new InvalidOperationException("Assign the authored Body on the Cat Warning View.");
            }

            _model = model;
            _eye = camera.transform;
            Subscribe();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void Subscribe()
        {
            if (_model == null || !isActiveAndEnabled)
            {
                return;
            }

            _model.StateChanged -= Render;
            _model.StateChanged += Render;
            Render();
        }

        private void OnDisable()
        {
            if (_model != null)
            {
                _model.StateChanged -= Render;
            }
        }

        private void Render()
        {
            _body.SetActive(_model.State == CatState.Warning);
        }

        private void LateUpdate()
        {
            if (_eye != null && _body.activeSelf)
            {
                transform.rotation = _eye.rotation;
            }
        }
    }
}
