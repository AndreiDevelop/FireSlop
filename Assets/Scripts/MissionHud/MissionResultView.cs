using System;
using FireSlop.Mission;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace FireSlop.MissionHud
{
    public sealed class MissionResultView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _time;
        [SerializeField] private Button _restart;

        private MissionModel _model;
        private MissionController _controller;

        [Inject]
        public void Construct([InjectOptional] MissionModel model, [InjectOptional] MissionController controller)
        {
            if (model == null)
            {
                _panel.SetActive(false);
                return;
            }

            _model = model;
            _controller = controller;
            if (isActiveAndEnabled)
            {
                Subscribe();
            }
        }

        private void OnEnable()
        {
            if (_model != null)
            {
                Subscribe();
            }
        }

        private void Subscribe()
        {
            _model.Won += Render;
            _restart.onClick.AddListener(Restart);
            Render();
        }

        private void Render()
        {
            _panel.SetActive(_model.IsWon);
            if (!_model.IsWon)
            {
                return;
            }

            var duration = TimeSpan.FromSeconds(_model.ElapsedSeconds);
            _time.text = $"{(int)duration.TotalMinutes:00}:{duration.Seconds:00}.{duration.Milliseconds / 10:00}";
        }

        private void Restart()
        {
            _controller.Restart();
        }

        private void OnDisable()
        {
            if (_model == null)
            {
                return;
            }

            _model.Won -= Render;
            _restart.onClick.RemoveListener(Restart);
        }
    }
}
