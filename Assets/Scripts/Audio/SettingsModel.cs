using System;

namespace FireSlop.Audio
{
    public sealed class SettingsModel
    {
        public float MusicVolume { get; private set; } = 1f;
        public float EffectsVolume { get; private set; } = 1f;

        public event Action LevelsChanged;

        public void SetLevels(float music, float effects)
        {
            music = Math.Clamp(music, 0f, 1f);
            effects = Math.Clamp(effects, 0f, 1f);
            if (music == MusicVolume && effects == EffectsVolume)
            {
                return;
            }

            MusicVolume = music;
            EffectsVolume = effects;
            LevelsChanged?.Invoke();
        }
    }
}
