using System;
using System.Collections.Generic;

namespace FireSlop.Interaction
{
    public sealed class ThrowPreviewModel
    {
        private readonly HashSet<object> _visibleSources = new HashSet<object>();

        public bool IsVisible => _visibleSources.Count > 0;

        public event Action VisibilityChanged;

        public void SetVisible(object source, bool visible)
        {
            var wasVisible = IsVisible;
            if (visible)
            {
                _visibleSources.Add(source);
            }
            else
            {
                _visibleSources.Remove(source);
            }

            if (wasVisible != IsVisible)
            {
                VisibilityChanged?.Invoke();
            }
        }
    }
}
