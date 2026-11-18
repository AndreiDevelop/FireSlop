namespace FireSlop.Interaction
{
    public sealed class DoorModel
    {
        public bool IsBroken { get; private set; }
        public int DamageHits { get; private set; }

        public bool RegisterHit(int hitsToBreak)
        {
            if (IsBroken)
            {
                return false;
            }

            DamageHits++;
            IsBroken = DamageHits >= hitsToBreak;
            return true;
        }
    }
}
