using FireSlop.Interaction;
using UnityEngine;

namespace FireSlop.Firefighter
{
    internal sealed class OpeningTraversal
    {
        private const float MinimumInputAlignment = 0.7071f;

        private readonly CharacterController _body;
        private readonly int _solidLayers;
        private readonly float _standingHeight;
        private readonly Vector3 _standingCenter;
        private Vector3 _direction;
        private float _targetY;
        private float _exitProjection;
        private Component _pendingOpening;
        private Vector3 _approachDirection;
        private float _approachTime;

        public float Height { get; private set; }
        public bool IsActive { get; private set; }

        public OpeningTraversal(CharacterController body)
        {
            _body = body;
            _standingHeight = body.height;
            _standingCenter = body.center;
            var playerLayer = body.gameObject.layer;
            for (var layer = 0; layer < 32; layer++)
            {
                if (layer != playerLayer && !Physics.GetIgnoreLayerCollision(playerLayer, layer))
                {
                    _solidLayers |= 1 << layer;
                }
            }

            // The carried ragdoll uses Ignore Raycast and must not obstruct clearance checks.
            _solidLayers &= Physics.DefaultRaycastLayers;
        }

        public void TryStart(Vector3 direction, float reach, MovementConfigSO config, bool crouchHeld, float deltaTime)
        {
            var opening = FindOpening(direction, reach, config, crouchHeld);
            if (opening == null || (_targetY <= _body.transform.position.y && Height >= _body.height))
            {
                Cancel();
                return;
            }

            if (_pendingOpening != opening || Vector3.Dot(direction, _approachDirection) < MinimumInputAlignment)
            {
                _pendingOpening = opening;
                _approachDirection = direction;
                _approachTime = 0f;
            }
            else
            {
                _approachTime += deltaTime;
            }

            IsActive = _approachTime >= config.AutoTraversalDelay;
        }

        private Component FindOpening(Vector3 direction, float reach, MovementConfigSO config, bool crouchHeld)
        {
            GetCapsule(_standingHeight, out var bottom, out var top);
            if (!Physics.CapsuleCast(bottom, top, Radius, direction, out var hit, reach,
                    _solidLayers, QueryTriggerInteraction.Ignore))
            {
                return null;
            }

            var feetY = _body.bounds.min.y;
            var crouchHeight = Mathf.Clamp(config.CrouchHeight, 2f * _body.radius, _standingHeight);
            // The bay and sill protrude beyond the window guard on either side.
            var windowReach = Mathf.Min(config.WindowJumpReach, reach + 2f * _body.radius);
            if (Physics.Raycast(top, direction, out var opening, windowReach,
                    config.WindowOpeningMask, QueryTriggerInteraction.Collide) &&
                opening.collider.GetComponentInParent<BreakableWindowView>() is { AllowsGroundTraversal: true })
            {
                var bounds = opening.collider.bounds;
                var rise = Mathf.Max(0f, bounds.min.y - feetY + _body.skinWidth);
                var windowHeight = Mathf.Clamp(config.WindowJumpCapsuleHeight, 2f * _body.radius, _standingHeight);
                return TryOpening(direction, bounds, rise, config.JumpHeight, windowHeight, crouchHeld)
                    ? opening.collider
                    : null;
            }

            if (!hit.collider.TryGetComponent<BreakableBrickView>(out var brick) || brick.Wall == null)
            {
                return null;
            }

            var wall = brick.Wall;
            var riseToTry = 0f;
            while (riseToTry <= config.JumpHeight)
            {
                if (TryOpening(direction, hit.collider.bounds, riseToTry, config.JumpHeight, crouchHeight,
                        crouchHeld))
                {
                    return wall;
                }

                riseToTry = wall.GetNextSillRise(feetY, riseToTry, _body.skinWidth);
            }

            return null;
        }

        public bool TryGetMotion(Vector3 input, float speed, float liftSpeed, float deltaTime, out Vector3 motion)
        {
            motion = Vector3.zero;
            if (!IsActive || Vector3.Dot(input.normalized, _direction) < MinimumInputAlignment)
            {
                Cancel();
                return false;
            }

            var rise = _targetY - _body.transform.position.y;
            if (rise > 0.001f)
            {
                motion = Vector3.up * Mathf.Min(rise, liftSpeed * deltaTime);
                return true;
            }

            motion = _direction * (input.magnitude * speed * deltaTime);
            return true;
        }

        public void FinishMove(Vector3 motion, CollisionFlags collision)
        {
            var blocked = motion.y > 0f
                ? (collision & CollisionFlags.Above) != 0
                : (collision & CollisionFlags.Sides) != 0;
            if (blocked || Vector3.Dot(_body.transform.position, _direction) >= _exitProjection)
            {
                Cancel();
            }
        }

        public void Cancel()
        {
            IsActive = false;
            _pendingOpening = null;
            _approachTime = 0f;
        }

        private float Radius => _body.radius - _body.skinWidth;

        private bool TryOpening(Vector3 direction, Bounds bounds, float rise, float maxRise, float crouchHeight,
            bool crouchHeld)
        {
            if (rise > maxRise)
            {
                return false;
            }

            var position = _body.transform.position;
            var exit = Vector3.Dot(bounds.center - position, direction) +
                Mathf.Abs(direction.x) * bounds.extents.x + Mathf.Abs(direction.z) * bounds.extents.z +
                _body.radius + _body.skinWidth;

            var height = crouchHeld ? crouchHeight : _standingHeight;
            if (!HasClearPath(direction, exit, rise, height))
            {
                height = crouchHeight;
                if (crouchHeld || !HasClearPath(direction, exit, rise, height))
                {
                    return false;
                }
            }

            Height = height;
            _direction = direction;
            _targetY = position.y + rise;
            _exitProjection = Vector3.Dot(position, direction) + exit;
            return true;
        }

        private bool HasClearPath(Vector3 direction, float distance, float rise, float height)
        {
            GetCapsule(height, out var bottom, out var top);
            if (rise > 0f && Physics.CapsuleCast(bottom, top, Radius, Vector3.up, rise,
                    _solidLayers, QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            var lift = Vector3.up * rise;
            bottom += lift;
            top += lift;
            return !Physics.CheckCapsule(bottom, top, Radius, _solidLayers, QueryTriggerInteraction.Ignore) &&
                !Physics.CapsuleCast(bottom, top, Radius, direction, distance,
                    _solidLayers, QueryTriggerInteraction.Ignore);
        }

        private void GetCapsule(float height, out Vector3 bottom, out Vector3 top)
        {
            var center = _body.transform.TransformPoint(_standingCenter) +
                Vector3.down * ((_standingHeight - height) * 0.5f);
            var offset = Vector3.up * (height * 0.5f - _body.radius);
            bottom = center - offset;
            top = center + offset;
        }
    }
}
