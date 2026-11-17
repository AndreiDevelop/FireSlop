using FireSlop.Drone;
using FireSlop.Interaction;
using FireSlop.MissionHud;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace FireSlop.Firefighter
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class FirefighterController : MonoBehaviour
    {
        [SerializeField] private Transform _eyes;

        private CharacterController _body;
        private DroneController _drone;
        private DroneModel _flight;
        private Transform _camera;
        private Vector3 _cameraLocalPosition;
        private Vector3[] _cameraChildPositions;
        private bool _wasPiloting;
        private MissionInputModel _input;
        private MovementConfigSO _config;
        private ToolSelectionModel _tools;
        private float _verticalSpeed;
        private float _pitch;
        private bool _waitForNeutral;
        private bool _focused;
        private bool _captureMouseOnFocus = true;
        private float _standingHeight;
        private Vector3 _standingCenter;
        private float _standingStepOffset;
        private Vector3 _standingEyes;
        private bool _windowJumpTuck;
        private float _jumpRequested;
        private float _jumpCameraVelocity;

        [Inject]
        public void Construct(MissionInputModel input, MovementConfigSO config, ToolSelectionModel tools,
            DroneModel flight, Camera camera, [InjectOptional] DroneController drone)
        {
            _input = input;
            _config = config;
            _tools = tools;
            _flight = flight;
            _drone = drone;
            _camera = camera.transform;
            _cameraLocalPosition = _camera.localPosition;
            _cameraChildPositions = new Vector3[_camera.childCount];
            for (var index = 0; index < _cameraChildPositions.Length; index++)
            {
                _cameraChildPositions[index] = _camera.GetChild(index).localPosition;
            }

            _flight.ContextChanged += RefreshFlightCamera;
        }

        private void Awake()
        {
            _body = GetComponent<CharacterController>();
            _standingHeight = _body.height;
            _standingCenter = _body.center;
            _standingStepOffset = _body.stepOffset;
            _standingEyes = _eyes.localPosition;
            _focused = Application.isFocused;
        }

        private void OnDestroy()
        {
            if (_flight != null)
            {
                _flight.ContextChanged -= RefreshFlightCamera;
            }
        }

        private void RefreshFlightCamera()
        {
            if (_wasPiloting != _flight.IsPiloted)
            {
                _wasPiloting = _flight.IsPiloted;
                ApplyFlightCamera();
            }
        }

        private void ApplyFlightCamera()
        {
            _camera.localPosition = _cameraLocalPosition;
            var cameraOffset = Vector3.zero;
            if (_flight.IsPiloted)
            {
                _camera.position = _drone.GetCameraPosition(_camera, _body);
                cameraOffset = _camera.InverseTransformVector(_camera.parent.TransformVector(
                    _camera.localPosition - _cameraLocalPosition));
            }

            for (var index = 0; index < _cameraChildPositions.Length; index++)
            {
                _camera.GetChild(index).localPosition = _cameraChildPositions[index] - cameraOffset;
            }
        }

        private void OnDisable()
        {
            ReleaseInput();
            SetHeight(_standingHeight);
            _windowJumpTuck = false;
            _eyes.localPosition = _standingEyes;
            _jumpCameraVelocity = 0f;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            _focused = hasFocus;
            if (hasFocus)
            {
                _captureMouseOnFocus = true;
            }
            else
            {
                ReleaseInput();
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                ReleaseInput();
            }
        }

        private void ReleaseInput()
        {
            _input?.Cancel();
            _jumpRequested = 0f;
            _waitForNeutral = true;
            _captureMouseOnFocus = false;
            Cursor.lockState = CursorLockMode.None;
        }

        private void Update()
        {
            if (_input == null || !_input.IsEnabled || !_focused)
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                ReleaseInput();
                return;
            }

            var mouse = Mouse.current;
            var captureMouse = _input.ConsumeMouseLookRequest();
            if (mouse != null && (_captureMouseOnFocus || captureMouse || mouse.rightButton.wasPressedThisFrame))
            {
                Cursor.lockState = CursorLockMode.Locked;
                _captureMouseOnFocus = false;
            }

            var look = _input.ConsumeLook() * (_config.DegreesPerScreen / Mathf.Max(1, Mathf.Min(Screen.width, Screen.height)));
            if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
            {
                look += mouse.delta.ReadValue() * _config.MouseSensitivity;
            }

            transform.Rotate(0f, look.x, 0f);
            _pitch = Mathf.Clamp(_pitch - look.y, -_config.PitchLimit, _config.PitchLimit);
            _eyes.localRotation = Quaternion.Euler(_pitch, 0f, 0f);

            var movement = ReadKeyboard(keyboard);
            if (_waitForNeutral && movement == Vector2.zero)
            {
                _waitForNeutral = false;
            }

            if (_waitForNeutral)
            {
                movement = Vector2.zero;
            }

            if (keyboard != null && keyboard.digit1Key.wasPressedThisFrame)
            {
                _tools.Select(0);
            }

            if (keyboard != null && keyboard.digit2Key.wasPressedThisFrame)
            {
                _tools.Select(1);
            }

            movement = Vector2.ClampMagnitude(movement + _input.Movement, 1f);
            if (_flight.IsPiloted)
            {
                _verticalSpeed = 0f;
                _jumpRequested = 0f;
                _windowJumpTuck = false;
                UpdateStance(false);
                var up = keyboard != null && keyboard.spaceKey.isPressed;
                var down = keyboard != null && (keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed);
                var vertical = Mathf.Clamp(_input.DroneElevation + (up ? 1f : 0f) - (down ? 1f : 0f), -1f, 1f);
                _drone.Fly(_body, movement, vertical, transform.eulerAngles.y, Time.deltaTime);
                ApplyFlightCamera();
                return;
            }

            _jumpRequested = Mathf.Max(0f, _jumpRequested - Time.deltaTime);
            if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
            {
                _jumpRequested = _config.JumpInputBuffer;
            }

            if (_body.isGrounded)
            {
                _verticalSpeed = Mathf.Max(0f, _verticalSpeed);
                _windowJumpTuck = false;

                if (_jumpRequested > 0f)
                {
                    _jumpRequested = 0f;
                    Jump();
                }
            }

            var crouchHeld = keyboard != null && (keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed);
            var direction = transform.right * movement.x + transform.forward * movement.y;
            var opening = FindOpenWindow(direction);
            var dockedWindow = opening != null && _drone != null && _drone.IsDockedAtWindow(opening);
            UpdateStance(crouchHeld, dockedWindow);
            var speed = _config.Speed;
            if (crouchHeld || (!_windowJumpTuck && _body.height < _standingHeight))
            {
                speed *= _config.CrouchSpeedMultiplier;
            }

            var velocity = direction * speed;
            _verticalSpeed -= _config.Gravity * Time.deltaTime;
            velocity.y = _verticalSpeed;
            var needsSillStep = dockedWindow && opening.bounds.min.y - _body.bounds.min.y > _standingStepOffset;
            if (needsSillStep)
            {
                _body.stepOffset = Mathf.Min(_config.JumpHeight, _body.height - _body.skinWidth);
            }

            var collision = _body.Move(velocity * Time.deltaTime);
            if (needsSillStep)
            {
                _body.stepOffset = _standingStepOffset;
            }
            if ((collision & CollisionFlags.Above) != 0 && _verticalSpeed > 0f)
            {
                _verticalSpeed = 0f;
            }

            UpdateStanceCamera();
            if (_drone != null)
            {
                _flight.SetOnPlatform(_drone.IsStandingOnDeck(_body));
            }
        }

        private void Jump()
        {
            _windowJumpTuck = FindOpenWindow(transform.forward) != null;
            _verticalSpeed = Mathf.Sqrt(2f * _config.Gravity * _config.JumpHeight);
        }

        private Collider FindOpenWindow(Vector3 direction)
        {
            var origin = transform.TransformPoint(_standingCenter);
            if (direction.sqrMagnitude <= 0f ||
                !Physics.Raycast(origin, direction, out var hit, _config.WindowJumpReach,
                    _config.WindowOpeningMask, QueryTriggerInteraction.Collide))
            {
                return null;
            }

            var window = hit.collider.GetComponentInParent<BreakableWindowView>();
            return window != null && window.IsOpen ? hit.collider : null;
        }

        private void UpdateStance(bool crouchHeld, bool walkingThroughWindow = false)
        {
            var height = crouchHeld ? _config.CrouchHeight : _standingHeight;
            if (_windowJumpTuck || walkingThroughWindow)
            {
                height = Mathf.Min(height, _config.WindowJumpCapsuleHeight);
            }

            height = Mathf.Clamp(height, _body.radius * 2f, _standingHeight);
            if (height < _body.height || (height > _body.height && CanFit(height)))
            {
                SetHeight(height);
            }
        }

        private bool CanFit(float height)
        {
            var center = transform.TransformPoint(CenterAtHeight(height));
            var endOffset = Vector3.up * (height * 0.5f - _body.radius);
            var solidLayers = Physics.DefaultRaycastLayers & ~(1 << gameObject.layer);
            return !Physics.CheckCapsule(center - endOffset, center + endOffset, _body.radius - _body.skinWidth,
                solidLayers, QueryTriggerInteraction.Ignore);
        }

        private Vector3 CenterAtHeight(float height)
        {
            return _standingCenter - Vector3.up * ((_standingHeight - height) * 0.5f);
        }

        private void SetHeight(float height)
        {
            _body.height = height;
            _body.center = CenterAtHeight(height);
        }

        private void UpdateStanceCamera()
        {
            var target = _standingEyes.y - (_standingHeight - _body.height);
            var position = _eyes.localPosition;
            if (Mathf.Abs(position.y - target) < 0.001f)
            {
                if (position.y != target)
                {
                    position.y = target;
                    _eyes.localPosition = position;
                }

                _jumpCameraVelocity = 0f;
                return;
            }

            position.y = Mathf.SmoothDamp(position.y, target, ref _jumpCameraVelocity, _config.JumpCameraSmoothTime);
            _eyes.localPosition = position;
        }

        private static Vector2 ReadKeyboard(Keyboard keyboard)
        {
            if (keyboard == null)
            {
                return Vector2.zero;
            }

            var right = keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed;
            var left = keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed;
            var forward = keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed;
            var backward = keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed;
            return new Vector2((right ? 1 : 0) - (left ? 1 : 0), (forward ? 1 : 0) - (backward ? 1 : 0));
        }
    }
}
