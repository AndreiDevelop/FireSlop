using FireSlop.Firefighter;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace FireSlop.MissionHud
{
    public sealed class GroundMovementButtonsView : MonoBehaviour
    {
        [SerializeField] private Button _jumpButton;

        private MissionInputModel _input;

        [Inject]
        public void Construct(MissionInputModel input)
        {
            _input = input;
            if (isActiveAndEnabled)
            {
                Bind();
            }
        }

        private void OnEnable()
        {
            if (_input != null)
            {
                Bind();
            }
        }

        private void OnDisable()
        {
            if (_input == null)
            {
                return;
            }

            _jumpButton.onClick.RemoveListener(_input.RequestJump);
        }

        private void Bind()
        {
            _jumpButton.onClick.AddListener(_input.RequestJump);
        }
    }
}
