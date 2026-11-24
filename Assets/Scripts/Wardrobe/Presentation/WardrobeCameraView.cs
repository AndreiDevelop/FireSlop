using System.Collections;
using UnityEngine;

namespace FireSlop.Wardrobe
{
    public sealed class WardrobeCameraView : WardrobeView
    {
        [SerializeField] private Camera _camera;
        private WardrobeFraming _target;
        private Coroutine _transition;

        protected override void Render()
        {
            WardrobeFraming framing = Model.Focus?.Framing ?? Model.Catalog.FullBody;
            if (_target == framing)
            {
                return;
            }

            bool animate = _target != null && Model.Catalog.CameraDuration > 0f;
            _target = framing;
            if (_transition != null)
            {
                StopCoroutine(_transition);
                _transition = null;
            }

            Transform cameraTransform = _camera.transform;
            Vector3 forward = cameraTransform.localRotation * Vector3.forward;
            Vector3 position = framing.Focus - forward * framing.Distance;
            if (animate)
            {
                _transition = StartCoroutine(Move(position, framing.Size));
                return;
            }

            cameraTransform.localPosition = position;
            _camera.orthographicSize = framing.Size;
        }

        private IEnumerator Move(Vector3 position, float size)
        {
            Vector3 start = _camera.transform.localPosition;
            float initialSize = _camera.orthographicSize;
            float elapsed = 0f;
            float duration = Model.Catalog.CameraDuration;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                _camera.transform.localPosition = Vector3.Lerp(start, position, t);
                _camera.orthographicSize = Mathf.Lerp(initialSize, size, t);
                yield return null;
            }

            _transition = null;
        }

        protected override void OnDisable()
        {
            if (_transition != null)
            {
                StopCoroutine(_transition);
                _transition = null;
            }

            _target = null;
            base.OnDisable();
        }
    }
}
