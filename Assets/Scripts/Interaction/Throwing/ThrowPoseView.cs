using System;
using UnityEngine;
using Zenject;

namespace FireSlop.Interaction
{
    public sealed class ThrowPoseView : MonoBehaviour
    {
        [SerializeField] private Transform _holdPoint;
        [SerializeField] private ThrowConfigSO _config;

        private Transform _eyes;

        public Transform HoldPoint => _holdPoint;
        public Vector3 EyePosition => _eyes.position;
        public Vector3 LaunchPosition => _eyes.TransformPoint(_config.Offset);
        public Vector3 LaunchVelocity => GetThrowVelocity(_eyes.forward);
        public float Radius => _config.Radius;

        public Vector3 GetThrowVelocity(Vector3 aim)
        {
            return _config.GetVelocity(aim);
        }

        [Inject]
        public void Construct(Camera camera)
        {
            if (_holdPoint == null || _config == null || !_config.IsValid)
            {
                throw new InvalidOperationException("Assign the authored throw hold point and valid throw settings.");
            }

            _eyes = camera.transform;
        }
    }
}
