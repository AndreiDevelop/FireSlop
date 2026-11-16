using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class InteractionConfigSO : ScriptableObject
    {
        [SerializeField, Min(0.1f)] private float _contextRange = 2.5f;
        [SerializeField, Min(0.01f)] private float _doorSwingDuration = 0.4f;

        public float ContextRange => _contextRange;
        public float DoorSwingDuration => _doorSwingDuration;
    }
}
