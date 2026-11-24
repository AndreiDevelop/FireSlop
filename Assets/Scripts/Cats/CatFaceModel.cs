using System;

namespace FireSlop.Cats
{
    public sealed class CatFaceModel
    {
        public string OccupantId { get; private set; }
        public bool IsOccupied => OccupantId != null;
        public bool IsAttached { get; private set; }
        public event Action Changed;
        public event Action RemovalRequested;

        public bool TryReserve(string contentId)
        {
            if (IsOccupied || string.IsNullOrWhiteSpace(contentId))
            {
                return false;
            }

            OccupantId = contentId;
            return true;
        }

        public void Attach(string contentId)
        {
            if (OccupantId == contentId && !IsAttached)
            {
                IsAttached = true;
                Changed?.Invoke();
            }
        }

        public void RequestRemoval()
        {
            if (IsAttached)
            {
                RemovalRequested?.Invoke();
            }
        }

        public void Release(string contentId)
        {
            if (OccupantId != contentId)
            {
                return;
            }

            OccupantId = null;
            IsAttached = false;
            Changed?.Invoke();
        }
    }
}
