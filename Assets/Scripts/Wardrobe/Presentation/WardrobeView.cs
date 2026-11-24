using UnityEngine;
using Zenject;

namespace FireSlop.Wardrobe
{
    public abstract class WardrobeView : MonoBehaviour
    {
        protected WardrobeModel Model { get; private set; }
        protected WardrobeController Controller { get; private set; }

        [Inject]
        public void Construct(WardrobeModel model, WardrobeController controller)
        {
            Model = model;
            Controller = controller;
            if (isActiveAndEnabled)
            {
                Subscribe();
            }
        }

        protected virtual void OnEnable()
        {
            if (Model != null)
            {
                Subscribe();
            }
        }

        protected virtual void OnDisable()
        {
            if (Model != null)
            {
                Model.Changed -= Render;
            }
        }

        private void Subscribe()
        {
            Model.Changed -= Render;
            Model.Changed += Render;
            Render();
        }

        protected abstract void Render();
    }
}
