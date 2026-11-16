using UnityEngine;

namespace FireSlop.Firefighter
{
    public sealed class MovementConfigSO : ScriptableObject
    {
        [SerializeField, Min(0.01f)] private float _speed;
        [SerializeField, Min(0.01f)] private float _gravity;
        [SerializeField, Min(0.01f)] private float _mouseSensitivity;
        [SerializeField, Min(0.01f)] private float _degreesPerScreen;
        [SerializeField, Range(1f, 89f)] private float _pitchLimit;

        public float Speed => _speed;
        public float Gravity => _gravity;
        public float MouseSensitivity => _mouseSensitivity;
        public float DegreesPerScreen => _degreesPerScreen;
        public float PitchLimit => _pitchLimit;
    }
}
