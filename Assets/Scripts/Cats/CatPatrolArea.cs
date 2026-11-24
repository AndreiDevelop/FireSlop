using Unity.AI.Navigation;
using UnityEngine;

namespace FireSlop.Cats
{
    public sealed class CatPatrolArea : MonoBehaviour
    {
        [SerializeField] private NavMeshSurface _surface;
        [SerializeField] private Transform[] _points;

        public int PointCount => _points.Length;
        public bool IsValid => _surface != null && _surface.navMeshData != null && _points != null &&
            _points.Length >= 2;

        public Vector3 GetPoint(int index)
        {
            return _points[index].position;
        }

        public bool Contains(Vector3 position)
        {
            var local = _surface.transform.InverseTransformPoint(position) - _surface.center;
            var half = _surface.size * 0.5f;
            return Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y && Mathf.Abs(local.z) <= half.z;
        }
    }
}
