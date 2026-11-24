using System;

namespace FireSlop.Dispatch
{
    public sealed class DispatchModel
    {
        public event Action Changed;
        public DispatchPanel Panel { get; private set; }
        public string Feedback { get; private set; }

        internal void Show(DispatchPanel panel)
        {
            Panel = panel;
            Feedback = null;
            Changed?.Invoke();
        }

        internal void Report(string message)
        {
            Feedback = message;
            Changed?.Invoke();
        }
    }
}
