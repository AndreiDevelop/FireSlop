using FireSlop.Mission;
using UnityEngine;
using Zenject;

namespace FireSlop.MissionHud
{
    public sealed class GameplayHudView : MonoBehaviour
    {
        [SerializeField] private GameObject _body;

        private MissionModel _model;

        [Inject]
        public void Construct([InjectOptional] MissionModel mission)
        {
            _model = mission;
            if (isActiveAndEnabled)
            {
                OnEnable();
            }
        }

        private void OnEnable()
        {
            if (_model != null)
            {
                _model.Won += Render;
                Render();
            }
        }

        private void OnDisable()
        {
            if (_model != null)
            {
                _model.Won -= Render;
            }
        }

        private void Render()
        {
            _body.SetActive(!_model.IsWon);
        }
    }
}
