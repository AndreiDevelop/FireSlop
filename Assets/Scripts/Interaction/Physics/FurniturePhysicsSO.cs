using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class FurniturePhysicsSO : ScriptableObject
    {
        [SerializeField, Min(0.01f)] private float _mass;
        [SerializeField, Min(0f)] private float _linearDamping;
        [SerializeField, Min(0f)] private float _angularDamping;

        public float Mass => _mass;
        public float LinearDamping => _linearDamping;
        public float AngularDamping => _angularDamping;
    }
}
