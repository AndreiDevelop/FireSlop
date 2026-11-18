namespace FireSlop.Interaction
{
    internal sealed class OpenableModel
    {
        public bool IsOpen { get; private set; }

        public bool IsLocked { get; private set; }

        public void Lock()
        {
            IsLocked = true;
        }

        public void Toggle()
        {
            IsOpen = !IsOpen;
        }
    }
}
