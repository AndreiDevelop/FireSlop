using FireSlop.Cats;
using FireSlop.Interaction;
using UnityEngine;
using Zenject;

namespace FireSlop.Firefighter
{
    public sealed class RescueHandsView : MonoBehaviour
    {
        [SerializeField] private GameObject _carryHands;
        [SerializeField] private GameObject _removalHands;

        private CatFaceModel _face;
        private InteractionModel _interaction;

        [Inject]
        public void Construct(CatFaceModel face, InteractionModel interaction)
        {
            _face = face;
            _interaction = interaction;
            if (isActiveAndEnabled)
            {
                Subscribe();
            }
        }

        private void OnEnable()
        {
            if (_face != null)
            {
                Subscribe();
            }
        }

        private void Subscribe()
        {
            _face.Changed += Render;
            _interaction.CarryingChanged += Render;
            Render();
        }

        private void OnDisable()
        {
            if (_face != null)
            {
                _face.Changed -= Render;
                _interaction.CarryingChanged -= Render;
            }
        }

        private void Render()
        {
            _carryHands.SetActive(_interaction.IsCarrying && !_face.IsAttached);
            _removalHands.SetActive(_face.IsAttached);
        }
    }
}
