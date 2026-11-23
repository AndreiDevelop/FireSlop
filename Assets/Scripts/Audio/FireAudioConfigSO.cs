using UnityEngine;

namespace FireSlop.Audio
{
    public sealed class FireAudioConfigSO : ScriptableObject
    {
        [SerializeField, Min(1)] private int _maxVoices = 8;
        [SerializeField, Min(0.01f)] private float _fadeSeconds = 0.2f;

        public int MaxVoices => _maxVoices;
        public float FadeSeconds => _fadeSeconds;
        public bool IsValid => _maxVoices > 0 && _fadeSeconds > 0f;
    }
}
