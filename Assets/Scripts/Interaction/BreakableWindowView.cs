using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class BreakableWindowView : MonoBehaviour
    {
        [SerializeField] private WindowSectionView _upperGlass;
        [SerializeField] private WindowSectionView _lowerGlass;
        [SerializeField] private WindowSectionView _wood;
        [SerializeField] private Collider _entryGuard;

        private readonly BreakableWindowModel _state = new BreakableWindowModel();

        public bool IsOpen => _state.IsOpen;

        internal void Hit(WindowSectionView section, Vector3 direction, InteractionConfigSO config)
        {
            if (section == _wood)
            {
                Release(_state.Break(WindowSectionKind.Wood), direction, config);
            }
            else if (section == _upperGlass)
            {
                Release(_state.Break(WindowSectionKind.UpperGlass), direction, config);
            }
            else if (section == _lowerGlass)
            {
                Release(_state.Break(WindowSectionKind.LowerGlass), direction, config);
            }
        }

        private void Release(WindowSectionKind broken, Vector3 direction, InteractionConfigSO config)
        {
            if ((broken & WindowSectionKind.Wood) != 0)
            {
                _wood.ReleaseFragments(transform, direction, config);
            }

            if ((broken & WindowSectionKind.UpperGlass) != 0)
            {
                _upperGlass.ReleaseFragments(transform, direction, config);
            }

            if ((broken & WindowSectionKind.LowerGlass) != 0)
            {
                _lowerGlass.ReleaseFragments(transform, direction, config);
            }

            if (IsOpen)
            {
                _entryGuard.isTrigger = true;
            }
        }
    }
}
