namespace FireSlop.Interaction
{
    internal sealed class BreakableWallModel
    {
        public bool IsBroken { get; private set; }

        public bool TryBreak()
        {
            if (IsBroken)
            {
                return false;
            }

            IsBroken = true;
            return true;
        }
    }
}
