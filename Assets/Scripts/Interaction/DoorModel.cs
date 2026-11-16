namespace FireSlop.Interaction
{
    public sealed class DoorModel
    {
        public DoorModel(bool isLocked)
        {
            IsLocked = isLocked;
        }

        public bool IsLocked { get; }
        public bool IsOpen { get; private set; }
        public bool IsBroken { get; private set; }
        public bool IsPassable => IsOpen || IsBroken;

        public bool TryToggle()
        {
            if (IsLocked || IsBroken)
            {
                return false;
            }

            IsOpen = !IsOpen;
            return true;
        }

        public bool Break()
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
