using UnityEngine;
using UnityEngine.AI;

namespace FireSlop.Interaction
{
    [RequireComponent(typeof(NavMeshObstacle))]
    public sealed class FurnitureNavigationView : MonoBehaviour
    {
        [SerializeField] private FurnitureNavigationConfigSO _config;

        private void Awake()
        {
            var obstacle = GetComponent<NavMeshObstacle>();
            if (_config == null || !_config.IsValid)
            {
                Debug.LogError("Assign valid Furniture Navigation settings on the movable furniture prefab.", this);
                obstacle.enabled = false;
                enabled = false;
                return;
            }

            obstacle.carving = true;
            obstacle.carveOnlyStationary = true;
            obstacle.carvingMoveThreshold = _config.MoveThreshold;
            obstacle.carvingTimeToStationary = _config.StationarySeconds;
        }
    }
}
