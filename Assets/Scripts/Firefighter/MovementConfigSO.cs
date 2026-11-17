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
        [SerializeField, Min(0.01f)] private float _crouchHeight = 1.05f;
        [SerializeField, Range(0.01f, 1f)] private float _crouchSpeedMultiplier = 0.55f;
        [SerializeField] private LayerMask _windowOpeningMask;

        [Header("Automatic Traversal")]
        [Tooltip("Seconds of continuous movement toward a passable opening before automatic traversal starts.")]
        [SerializeField, Min(0f)] private float _autoTraversalDelay;

        [Header("Hose Flight")]
        [SerializeField, Range(0f, 45f)] private float _hoseLaunchAngle = 12f;
        [SerializeField, Range(0f, 75f)] private float _hoseSteeringAngle = 40f;
        [SerializeField, Min(0.01f)] private float _hoseMaxRise = 4f;
        [SerializeField, Min(0.01f)] private float _hoseSupportRange = 8f;
        [SerializeField] private LayerMask _hoseSupportMask = 1;
        [SerializeField, Range(0f, 1f)] private float _hoseSupportMinUp = 0.55f;
        [SerializeField, Min(0.01f)] private float _hoseLiftSpeed = 3.6f;
        [SerializeField, Min(0.01f)] private float _hoseSteeringSpeed = 3.2f;
        [SerializeField, Min(0.01f)] private float _hoseSteeringAcceleration = 12f;
        [SerializeField, Min(0.01f)] private float _hoseRecoveryAcceleration = 15f;
        [SerializeField, Min(0.01f)] private float _hoseHoverDuration = 0.85f;
        [SerializeField, Min(0.01f)] private float _hoseFallDuration = 1.05f;
        [SerializeField, Min(0.01f)] private float _hoseFallSpeed = 9f;
        [SerializeField] private AnimationCurve _hoseLiftCurve = new AnimationCurve(
            new Keyframe(0f, 0.35f), new Keyframe(0.35f, 1f), new Keyframe(0.72f, 1f), new Keyframe(1f, 0.22f));
        [SerializeField] private AnimationCurve _hoseHoverCurve = new AnimationCurve(
            new Keyframe(0f, 1f), new Keyframe(0.24f, 0f), new Keyframe(1f, 0f));
        [SerializeField] private AnimationCurve _hoseFallCurve = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 1f, 2f, 0f));

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
        public float CrouchHeight => _crouchHeight;
        public float CrouchSpeedMultiplier => _crouchSpeedMultiplier;
        public LayerMask WindowOpeningMask => _windowOpeningMask;
        public float AutoTraversalDelay => _autoTraversalDelay;
        public float HoseLaunchAngle => _hoseLaunchAngle;
        public float HoseSteeringAngle => _hoseSteeringAngle;
        public float HoseMaxRise => _hoseMaxRise;
        public float HoseSupportRange => _hoseSupportRange;
        public LayerMask HoseSupportMask => _hoseSupportMask;
        public float HoseSupportMinUp => _hoseSupportMinUp;
        public float HoseLiftSpeed => _hoseLiftSpeed;
        public float HoseSteeringSpeed => _hoseSteeringSpeed;
        public float HoseSteeringAcceleration => _hoseSteeringAcceleration;
        public float HoseRecoveryAcceleration => _hoseRecoveryAcceleration;
        public float HoseHoverDuration => _hoseHoverDuration;
        public float HoseFallDuration => _hoseFallDuration;
        public float HoseFallSpeed => _hoseFallSpeed;

        public float EvaluateHoseLift(float progress)
        {
            return Mathf.Clamp01(_hoseLiftCurve.Evaluate(progress));
        }

        public float EvaluateHoseHover(float progress)
        {
            return Mathf.Clamp01(_hoseHoverCurve.Evaluate(progress));
        }

        public float EvaluateHoseFall(float progress)
        {
            return Mathf.Clamp01(_hoseFallCurve.Evaluate(progress));
        }
    }
}
