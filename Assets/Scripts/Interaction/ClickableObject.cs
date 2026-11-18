using UnityEngine;

namespace FireSlop.Interaction
{
    [DisallowMultipleComponent]
    public sealed class ClickableObject : MonoBehaviour
    {
        [SerializeField] private Sprite _actionIcon;

        public Sprite ActionIcon => _actionIcon;
    }
}
