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
        private MissionInputModel _input;
        private MovementConfigSO _config;
        private ToolSelectionModel _tools;
        private float _verticalSpeed;
        private float _pitch;
        private bool _waitForNeutral;
        private bool _focused;

        [Inject]
        public void Construct(MissionInputModel input, MovementConfigSO config, ToolSelectionModel tools)
        {
            _input = input;
            _config = config;
            _tools = tools;
        }

        private void Awake()
        {
            _body = GetComponent<CharacterController>();
            _focused = Application.isFocused;
        }

        private void OnDisable()
        {
            ReleaseInput();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            _focused = hasFocus;
            if (!hasFocus)
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
            _waitForNeutral = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void Update()
        {
            if (_input == null || !_focused)
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
            if (mouse != null && mouse.rightButton.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.Locked;
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
            var velocity = (transform.right * movement.x + transform.forward * movement.y) * _config.Speed;
            if (_body.isGrounded && _verticalSpeed < 0f)
            {
                _verticalSpeed = 0f;
            }

            _verticalSpeed -= _config.Gravity * Time.deltaTime;
            velocity.y = _verticalSpeed;
            _body.Move(velocity * Time.deltaTime);
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
