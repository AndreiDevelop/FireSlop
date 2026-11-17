using System;

namespace FireSlop.Interaction
{
    internal sealed class BreakableWallModel
    {
        private readonly WallFragmentDamage[] _fragments;

        public BreakableWallModel(int fragmentCount)
        {
            _fragments = new WallFragmentDamage[fragmentCount];
        }

        public bool IsBroken { get; private set; }
        public int DamageHits { get; private set; }
        public int DetachedFragments { get; private set; }

        public bool TryHit(int index, int fragmentsToCollapse, out WallFragmentDamage damage)
        {
            damage = _fragments[index];
            if (IsBroken || damage == WallFragmentDamage.Detached)
            {
                return false;
            }

            DamageHits++;
            damage = damage == WallFragmentDamage.Intact ? WallFragmentDamage.Dented : WallFragmentDamage.Detached;
            _fragments[index] = damage;
            if (damage == WallFragmentDamage.Detached)
            {
                DetachedFragments++;
                IsBroken = DetachedFragments >= Math.Min(fragmentsToCollapse, _fragments.Length);
            }

            return true;
        }
    }
}
