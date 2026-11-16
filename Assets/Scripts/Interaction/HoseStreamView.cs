using UnityEngine;
using Zenject;

namespace FireSlop.Interaction
{
    public sealed class HoseStreamView : MonoBehaviour
    {
        [SerializeField] private LineRenderer _stream;
        [SerializeField] private Renderer _impact;

        private InteractionModel _model;

        [Inject]
        public void Construct(InteractionModel model)
        {
            _model = model;
            if (isActiveAndEnabled)
            {
                _model.HoseStopped += Hide;
            }
        }

        private void OnEnable()
        {
            if (_model != null)
            {
                _model.HoseStopped += Hide;
            }

            Hide();
        }

        private void OnDisable()
        {
            if (_model != null)
            {
                _model.HoseStopped -= Hide;
            }

            Hide();
        }

        public void Render(Vector3 origin, Vector3 end, Vector3 surfaceNormal)
        {
            _stream.SetPosition(0, origin);
            _stream.SetPosition(1, end);
            _stream.enabled = true;

            var hasImpact = surfaceNormal != Vector3.zero;
            _impact.enabled = hasImpact;
            if (hasImpact)
            {
                var impact = _impact.transform;
                impact.SetPositionAndRotation(end, Quaternion.FromToRotation(Vector3.up, surfaceNormal));
            }
        }

        public void Hide()
        {
            _stream.enabled = false;
            _impact.enabled = false;
        }
    }
}
