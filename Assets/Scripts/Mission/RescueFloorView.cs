using FireSlop.Interaction;
using UnityEngine;
using Zenject;

namespace FireSlop.Mission
{
    public sealed class RescueFloorView : MonoBehaviour
    {
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        [SerializeField] private Renderer _floor;
        [SerializeField, Min(0.1f)] private float _pulseSeconds = 1.6f;
        [SerializeField, Range(0f, 1f)] private float _minimumOpacity = 0.35f;

        private MaterialPropertyBlock _properties;
        private RescueController _carrier;
        private Color _tint;

        [Inject]
        public void Construct(RescueController carrier)
        {
            _carrier = carrier;
            _properties = new MaterialPropertyBlock();
            _tint = _floor.sharedMaterial.GetColor(BaseColor);
        }

        private void LateUpdate()
        {
            _floor.enabled = _carrier != null && _carrier.Carried is IRescueTarget;
            if (!_floor.enabled)
            {
                return;
            }

            var phase = Time.time * (2f * Mathf.PI / _pulseSeconds);
            var color = _tint;
            color.a *= Mathf.Lerp(_minimumOpacity, 1f, 0.5f + 0.5f * Mathf.Sin(phase));
            _properties.SetColor(BaseColor, color);
            _floor.SetPropertyBlock(_properties);
        }
    }
}
