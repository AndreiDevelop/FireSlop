using UnityEngine;

namespace FireSlop.Wardrobe
{
    public sealed class WardrobeGripView : WardrobeView
    {
        [SerializeField] private MeshFilter _glove;
        [SerializeField] private MeshFilter _sleeve;
        [SerializeField] private Mesh[] _hands;
        [SerializeField] private Mesh[] _outfits;

        protected override void Render()
        {
            _glove.sharedMesh = FindEquippedMesh(_hands);
            _sleeve.sharedMesh = FindEquippedMesh(_outfits);
        }

        private Mesh FindEquippedMesh(Mesh[] meshes)
        {
            foreach (Mesh mesh in meshes)
            {
                if (Model.Wears(mesh.name))
                {
                    return mesh;
                }
            }

            return null;
        }
    }
}
