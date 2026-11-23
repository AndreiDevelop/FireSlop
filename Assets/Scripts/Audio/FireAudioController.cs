using System.Collections.Generic;
using FireSlop.Interaction;
using UnityEngine;
using Zenject;

namespace FireSlop.Audio
{
    public sealed class FireAudioController : ITickable
    {
        private readonly FireAudioModel _model;
        private readonly FireAudioConfigSO _config;
        private readonly Camera _camera;
        private readonly List<WaterFireTarget> _fires = new List<WaterFireTarget>();
        private readonly List<float> _distances = new List<float>();
        private readonly List<bool> _playing = new List<bool>();

        public FireAudioController(FireAudioModel model, FireAudioConfigSO config, Camera camera)
        {
            _model = model;
            _config = config;
            _camera = camera;
        }

        public void Register(WaterFireTarget fire)
        {
            if (!_fires.Contains(fire))
            {
                _fires.Add(fire);
            }
        }

        public void Unregister(WaterFireTarget fire)
        {
            _fires.Remove(fire);
        }

        public void Tick()
        {
            _distances.Clear();
            _playing.Clear();
            var hasListener = _camera != null && _camera.isActiveAndEnabled;
            var position = hasListener ? _camera.transform.position : Vector3.zero;
            foreach (var fire in _fires)
            {
                _distances.Add(hasListener ? fire.GetAudioDistanceSquared(position) : float.PositiveInfinity);
                _playing.Add(fire.IsAudioPlaying);
            }

            _model.Select(_distances, _playing);
            var fadeStep = Time.unscaledDeltaTime / _config.FadeSeconds;
            for (var index = 0; index < _fires.Count; index++)
            {
                _fires[index].RenderAudio(_model.IsAudible(index), fadeStep);
            }
        }
    }
}
