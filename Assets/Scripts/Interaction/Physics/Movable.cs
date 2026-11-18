using UnityEngine;

namespace FireSlop.Interaction
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class Movable : MonoBehaviour
    {
        [SerializeField] private FurniturePhysicsSO _settings;
        [SerializeField] private string _contentId;

        public string ContentId => _contentId;

        private void Awake()
        {
            var body = GetComponent<Rigidbody>();
            if (_settings == null || _settings.Mass <= 0f || string.IsNullOrWhiteSpace(_contentId))
            {
                Debug.LogError("Movable requires valid physics settings and an authored content ID.", this);
                body.isKinematic = true;
                return;
            }

            body.mass = _settings.Mass;
            body.linearDamping = _settings.LinearDamping;
            body.angularDamping = _settings.AngularDamping;
        }
    }
}
