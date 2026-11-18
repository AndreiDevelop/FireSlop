namespace FireSlop.Interaction
{
    internal sealed class OpenableModel
    {
        public bool IsOpen { get; private set; }

        public void Toggle()
        {
            IsOpen = !IsOpen;
        }
    }
}
