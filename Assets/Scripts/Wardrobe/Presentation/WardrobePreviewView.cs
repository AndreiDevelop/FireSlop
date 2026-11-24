using UnityEngine;

namespace FireSlop.Wardrobe
{
    public sealed class WardrobePreviewView : WardrobeView
    {
        [SerializeField] private GameObject[] _parts;

        protected override void Render()
        {
            foreach (GameObject part in _parts)
            {
                part.SetActive(Model.Wears(part.name));
            }
        }
    }
}
