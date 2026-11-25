using UnityEngine;

namespace FireSlop.Mission
{
    public sealed class RescueTentView : MonoBehaviour
    {
        [SerializeField] private Transform[] _seats;
        [SerializeField] private Transform[] _catRoutes;

        private int _nextRoute;

        public Transform GetSeat(int rescueIndex)
        {
            return _seats[rescueIndex];
        }

        public Transform TakeCatRoute()
        {
            var route = _catRoutes[_nextRoute];
            _nextRoute = (_nextRoute + 1) % _catRoutes.Length;
            return route;
        }
    }
}
