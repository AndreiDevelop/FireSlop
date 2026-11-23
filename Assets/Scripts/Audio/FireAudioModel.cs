using System.Collections.Generic;

namespace FireSlop.Audio
{
    public sealed class FireAudioModel
    {
        private readonly int _maxVoices;
        private readonly List<int> _nearest;
        private readonly List<int> _audible;

        public FireAudioModel(FireAudioConfigSO config)
        {
            _maxVoices = config.MaxVoices;
            _nearest = new List<int>(_maxVoices + 1);
            _audible = new List<int>(_maxVoices);
        }

        public void Select(IReadOnlyList<float> distances, IReadOnlyList<bool> playing)
        {
            _nearest.Clear();
            _audible.Clear();
            var available = _maxVoices;
            for (var index = 0; index < distances.Count; index++)
            {
                if (playing[index])
                {
                    available--;
                }

                if (float.IsPositiveInfinity(distances[index]))
                {
                    continue;
                }

                var position = 0;
                while (position < _nearest.Count && distances[_nearest[position]] <= distances[index])
                {
                    position++;
                }

                if (position < _maxVoices)
                {
                    _nearest.Insert(position, index);
                    if (_nearest.Count > _maxVoices)
                    {
                        _nearest.RemoveAt(_maxVoices);
                    }
                }
            }

            // Fading voices keep their slots until playback stops.
            foreach (var index in _nearest)
            {
                if (playing[index])
                {
                    _audible.Add(index);
                }
                else if (available > 0)
                {
                    _audible.Add(index);
                    available--;
                }
            }
        }

        public bool IsAudible(int index)
        {
            return _audible.Contains(index);
        }
    }
}
