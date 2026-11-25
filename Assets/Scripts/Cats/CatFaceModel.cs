using System;

namespace FireSlop.Cats
{
    public sealed class CatFaceModel
    {
        private double _presses;
        private float _drainSeconds;
        private int _requiredPresses;
        private double _nextDecayTime;
        private float _gapSeconds;

        public float Progress => IsAttached ? (float)(_presses / _requiredPresses) : 0f;
        public event Action ProgressChanged;
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

        public void Attach(string contentId, CatConfigSO config)
        {
            if (OccupantId == contentId && !IsAttached)
            {
                _requiredPresses = config.RemovalPresses;
                _gapSeconds = config.RemovalGapSeconds;
                _drainSeconds = config.RemovalDrainSeconds;
                IsAttached = true;
                Changed?.Invoke();
            }
        }

        public void RequestRemoval(double now)
        {
            if (!IsAttached || _presses >= _requiredPresses)
            {
                return;
            }

            AdvanceDecay(now);
            _nextDecayTime = now + _gapSeconds;
            _presses = Math.Min(_requiredPresses, _presses + 1d);
            ProgressChanged?.Invoke();
            if (_presses == _requiredPresses)
            {
                RemovalRequested?.Invoke();
            }
        }

        public void AdvanceDecay(double now)
        {
            if (_presses > 0d && now > _nextDecayTime)
            {
                var elapsed = now - _nextDecayTime;
                _presses = Math.Max(0d, _presses - elapsed * _requiredPresses / _drainSeconds);
                _nextDecayTime = now;
                ProgressChanged?.Invoke();
            }
        }

        public void Release(string contentId)
        {
            if (OccupantId != contentId)
            {
                return;
            }

            _presses = 0;
            OccupantId = null;
            IsAttached = false;
            Changed?.Invoke();
        }
    }
}
