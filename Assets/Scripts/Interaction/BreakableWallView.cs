using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class BreakableWallView : MonoBehaviour
    {
        [SerializeField] private string _contentId;
        [SerializeField] private Collider _solid;
        [SerializeField] private Renderer _wall;
        [SerializeField] private BreakableFragment[] _fragments;

        private readonly BreakableWallModel _state = new BreakableWallModel();

        public string ContentId => _contentId;
        public bool IsBroken => _state.IsBroken;

        internal void Hit(Vector3 direction, InteractionConfigSO config)
        {
            if (!_state.TryBreak())
            {
                return;
            }

            _solid.enabled = false;
            _wall.enabled = false;
            foreach (var fragment in _fragments)
            {
                fragment.Detach(transform, direction, config);
            }
        }
    }
}
