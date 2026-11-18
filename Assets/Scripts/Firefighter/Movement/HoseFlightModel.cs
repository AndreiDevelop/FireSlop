using UnityEngine;

namespace FireSlop.Firefighter
{
    public sealed class HoseFlightModel
    {
        private readonly MovementConfigSO _config;
        private float _launchHeight;
        private float _timeWithoutThrust;
        private float _releaseSpeed;
        private bool _hadThrust;

        public HoseFlightModel(MovementConfigSO config)
        {
            _config = config;
        }

        public bool IsAirborne { get; private set; }
        public Vector3 Velocity { get; private set; }

        public void Reset()
        {
            IsAirborne = false;
            Velocity = Vector3.zero;
            _hadThrust = false;
        }

        public void Start(float height)
        {
            _launchHeight = height;
            _timeWithoutThrust = 0f;
            IsAirborne = true;
        }

        public void Advance(float height, bool hasThrust, Vector3 steeringVelocity, float deltaTime)
        {
            var horizontal = Vector3.ProjectOnPlane(Velocity, Vector3.up);
            horizontal = Vector3.MoveTowards(horizontal, steeringVelocity, _config.HoseSteeringAcceleration * deltaTime);
            var vertical = hasThrust ? Rise(height, deltaTime) : Drift(deltaTime);
            Velocity = horizontal + Vector3.up * vertical;
            _hadThrust = hasThrust;
        }

        public void StopAtCollision(CollisionFlags collision)
        {
            if ((collision & CollisionFlags.Above) != 0 && Velocity.y > 0f)
            {
                Velocity = Vector3.ProjectOnPlane(Velocity, Vector3.up);
            }

            if ((collision & CollisionFlags.Below) != 0 && Velocity.y <= 0f)
            {
                Reset();
            }
        }

        private float Rise(float height, float deltaTime)
        {
            var maxHeight = _launchHeight + _config.HoseMaxRise;
            var progress = Mathf.Clamp01((height - _launchHeight) / _config.HoseMaxRise);
            var targetSpeed = _config.HoseLiftSpeed * _config.EvaluateHoseLift(progress);
            var speed = Mathf.MoveTowards(Velocity.y, targetSpeed, _config.HoseRecoveryAcceleration * deltaTime);
            var remainingSpeed = Mathf.Max(0f, maxHeight - height) / deltaTime;
            return Mathf.Min(speed, remainingSpeed);
        }

        private float Drift(float deltaTime)
        {
            if (_hadThrust)
            {
                _timeWithoutThrust = 0f;
                _releaseSpeed = Velocity.y;
            }

            _timeWithoutThrust += deltaTime;
            if (_releaseSpeed >= 0f && _timeWithoutThrust < _config.HoseHoverDuration)
            {
                return _releaseSpeed * _config.EvaluateHoseHover(_timeWithoutThrust / _config.HoseHoverDuration);
            }

            var fallTime = _timeWithoutThrust;
            if (_releaseSpeed >= 0f)
            {
                fallTime -= _config.HoseHoverDuration;
            }

            var fallProgress = Mathf.Clamp01(fallTime / _config.HoseFallDuration);
            var initialFallSpeed = Mathf.Max(0f, -_releaseSpeed);
            return -Mathf.Lerp(initialFallSpeed, _config.HoseFallSpeed, _config.EvaluateHoseFall(fallProgress));
        }
    }
}
