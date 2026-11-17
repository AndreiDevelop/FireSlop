using UnityEngine;

namespace FireSlop.Drone
{
    public sealed class DroneRotorView : MonoBehaviour
    {
        [SerializeField] private Transform[] _rotors;
        [SerializeField, Min(0f)] private float _degreesPerSecond = 1500f;

        private void Awake()
        {
            if (_rotors != null && _rotors.Length == 4 &&
                _rotors[0] != null && _rotors[1] != null && _rotors[2] != null && _rotors[3] != null)
            {
                return;
            }

            Debug.LogError("Drone rotor animation requires four assigned rotor transforms.", this);
            enabled = false;
        }

        internal void Spin(float deltaTime)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            var angle = _degreesPerSecond * deltaTime;
            for (var index = 0; index < _rotors.Length; index++)
            {
                _rotors[index].Rotate(Vector3.up, angle, Space.Self);
                angle = -angle;
            }
        }
    }
}
