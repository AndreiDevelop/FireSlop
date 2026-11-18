using UnityEngine;

namespace FireSlop.Drone
{
    [CreateAssetMenu(menuName = "Fire Slop/Drone Config")]
    public sealed class DroneConfigSO : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _hoverAfterExitSeconds = 10f;
        [SerializeField, Min(0.1f)] private float _horizontalSpeed = 4f;
        [SerializeField, Min(0.1f)] private float _verticalSpeed = 2.5f;
        [SerializeField, Min(0.1f)] private float _descentSpeed = 1.1f;
        [SerializeField, Min(1f)] private float _flightRadius = 45f;
        [SerializeField, Min(1f)] private float _flightHeight = 16f;
        [SerializeField, Min(0.1f)] private float _cameraDistance = 3.5f;
        [SerializeField, Min(0f)] private float _cameraHeight = 0.75f;
        [SerializeField, Min(0.01f)] private float _cameraTransitionSeconds = 0.45f;
        [SerializeField, Min(0.05f)] private float _windowDockTolerance = 0.4f;

        [SerializeField, Min(1f)] private float _approachSpeed = 12f;
        [SerializeField, Min(0.1f)] private float _arrivalDescentSpeed = 3f;
        [SerializeField, Min(1f)] private float _approachHeight = 21f;
        [SerializeField, Min(1f)] private float _throwSpeed = 9f;
        [SerializeField, Min(0f)] private float _throwLift = 2.5f;

        [SerializeField] private Vector3 _throwOffset = new Vector3(0.20f, -0.16f, 0.30f);

        public Vector3 ThrowOffset => _throwOffset;
        public float ApproachSpeed => _approachSpeed;
        public float ArrivalDescentSpeed => _arrivalDescentSpeed;
        public float ApproachHeight => _approachHeight;
        public float ThrowSpeed => _throwSpeed;
        public float ThrowLift => _throwLift;
        public float HoverAfterExitSeconds => _hoverAfterExitSeconds;
        public float HorizontalSpeed => _horizontalSpeed;
        public float VerticalSpeed => _verticalSpeed;
        public float DescentSpeed => _descentSpeed;
        public float FlightRadius => _flightRadius;
        public float FlightHeight => _flightHeight;
        public float CameraDistance => _cameraDistance;
        public float CameraHeight => _cameraHeight;
        public float CameraTransitionSeconds => _cameraTransitionSeconds;
        public float WindowDockTolerance => _windowDockTolerance;
    }
}
