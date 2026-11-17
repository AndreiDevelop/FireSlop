using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class BreakableWallView : MonoBehaviour
    {
        [SerializeField] private string _contentId;

        public string ContentId => _contentId;

    }
}
