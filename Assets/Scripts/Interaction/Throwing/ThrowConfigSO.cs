using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class ThrowConfigSO : ScriptableObject
    {
        [SerializeField, Min(0.1f)] private float _speed = 9f;
        [SerializeField, Min(0f)] private float _lift = 2.5f;
        [SerializeField] private Vector3 _offset = new Vector3(0.20f, -0.16f, 0.30f);
        [SerializeField, Min(0.001f)] private float _radius = 0.06f;

        public Vector3 Offset => _offset;
        public float Radius => _radius;
        public bool IsValid => _speed > 0f && _lift >= 0f && _radius > 0f;

        public Vector3 GetVelocity(Vector3 forward)
        {
            return forward * _speed + Vector3.up * _lift;
        }
    }
}
