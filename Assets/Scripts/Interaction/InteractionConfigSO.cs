using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class InteractionConfigSO : ScriptableObject
    {
        [SerializeField, Min(0.1f)] private float _contextRange = 2.5f;

        public float ContextRange => _contextRange;
    }
}
