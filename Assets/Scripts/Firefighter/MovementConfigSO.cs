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
        [SerializeField, Min(0.01f)] private float _jumpHeight = 1.2f;
        [SerializeField, Min(0.01f)] private float _jumpInputBuffer = 0.12f;
        [SerializeField, Min(0.01f)] private float _jumpCameraSmoothTime = 0.08f;
        [SerializeField, Min(0.01f)] private float _windowJumpReach = 1.8f;
        [SerializeField, Min(0.01f)] private float _windowJumpCapsuleHeight = 1.05f;
        [SerializeField] private LayerMask _windowOpeningMask;

        public float Speed => _speed;
        public float Gravity => _gravity;
        public float MouseSensitivity => _mouseSensitivity;
        public float DegreesPerScreen => _degreesPerScreen;
        public float PitchLimit => _pitchLimit;
        public float JumpHeight => _jumpHeight;
        public float JumpInputBuffer => _jumpInputBuffer;
        public float JumpCameraSmoothTime => _jumpCameraSmoothTime;
        public float WindowJumpReach => _windowJumpReach;
        public float WindowJumpCapsuleHeight => _windowJumpCapsuleHeight;
        public LayerMask WindowOpeningMask => _windowOpeningMask;
    }
}
