using Unity.Profiling;
using UnityEngine;
using Zenject;

namespace FireSlop.Interaction
{
    public sealed class FireSpreadController : MonoBehaviour
    {
        private static readonly ProfilerMarker SpreadMarker = new ProfilerMarker("Fire.Spread");

        [SerializeField] private WaterFireTarget _seed;
        [SerializeField] private WaterFireTarget _additionalSeed;
        [SerializeField] private WaterFireTarget _firePrefab;
        [SerializeField] private Collider[] _floors;
        [SerializeField] private FireSpreadConfigSO _config;

        private FireSpreadModel _model;
        private IInstantiator _instantiator;
        private FireFloorTopology _floor;
        private WaterFireTarget[] _views;
        private double _nextTick;
        private int _nextLink;

        [Inject]
        internal void Construct(FireSpreadModel model, IInstantiator instantiator)
        {
            _model = model;
            _instantiator = instantiator;
        }

        private void Start()
        {
            if (!HasValidSceneReferences())
            {
                Debug.LogError("Fire spread requires an initialized seed, its matching prefab, floors, and valid settings.",
                    this);
                enabled = false;
                return;
            }

            Physics.SyncTransforms();
            Vector3? additionalPosition = _additionalSeed != null ? _additionalSeed.transform.position : null;
            _floor = FireFloorTopology.Build(_seed.transform.position, additionalPosition, _floors, _config);
            if (_floor == null)
            {
                Debug.LogError("Fire spread seeds must occupy distinct cells on the assigned floor colliders.", this);
                enabled = false;
                return;
            }

            _views = new WaterFireTarget[_floor.CellCount];
            _views[_floor.SeedIndex] = _seed;
            _seed.EnableReignition(_config.ReignitionDelay);
            _model.Initialize(_floor.CellCount, _floor.SeedIndex, _seed.Fire);
            if (_additionalSeed != null)
            {
                _views[_floor.AdditionalSeedIndex] = _additionalSeed;
                _additionalSeed.EnableReignition(_config.ReignitionDelay);
                _model.AddStartingFire(_floor.AdditionalSeedIndex, _additionalSeed.Fire);
            }

            _nextTick = Time.timeAsDouble + _config.SpreadInterval;
        }

        private bool HasValidSceneReferences()
        {
            if (_model == null || _firePrefab == null || !IsValidSeed(_seed) ||
                _config == null || !_config.IsValid ||
                _floors == null || _floors.Length == 0)
            {
                return false;
            }

            if (_additionalSeed != null && (_additionalSeed == _seed || !IsValidSeed(_additionalSeed)))
            {
                return false;
            }

            foreach (var floor in _floors)
            {
                if (floor == null || !floor.enabled || !floor.gameObject.activeInHierarchy)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsValidSeed(WaterFireTarget seed)
        {
            return seed != null && seed.Fire != null && seed.Settings == _firePrefab.Settings;
        }

        private void Update()
        {
            var time = Time.timeAsDouble;
            if (time < _nextTick || !_model.HasBurningCells)
            {
                return;
            }

            using (SpreadMarker.Auto())
            {
                Spread(time);
            }

            _nextTick = time + _config.SpreadInterval;
        }

        private void Spread(double time)
        {
            var links = _floor.Links;
            var remaining = SpreadThroughBrokenDoors(time);
            for (var checkedCount = 0; checkedCount < links.Count && remaining > 0; checkedCount++)
            {
                var link = links[_nextLink];
                _nextLink = (_nextLink + 1) % links.Count;
                if (TrySpread(link, time))
                {
                    remaining--;
                }
            }
        }

        private int SpreadThroughBrokenDoors(double time)
        {
            var remaining = _config.IgnitionsPerTick;
            var doorLinks = _floor.DoorLinks;
            for (var index = 0; index < doorLinks.Count && remaining > 0; index++)
            {
                var link = doorLinks[index];
                if (link.HasBrokenDoor() && TrySpread(link, time))
                {
                    remaining--;
                }
            }

            return remaining;
        }

        private bool TrySpread(FireSpreadLink link, double time)
        {
            return TrySpread(link, link.First, link.Second, time) || TrySpread(link, link.Second, link.First, time);
        }

        private bool TrySpread(FireSpreadLink link, int source, int destination, double time)
        {
            if (!_model.CanSpread(source, time) || !_model.CanIgnite(destination, time) || !link.IsOpen())
            {
                return false;
            }

            var point = _floor.PositionAt(destination);
            var center = point + Vector3.up * _config.ClearanceHeight;
            if (Physics.CheckSphere(center, _config.ClearanceRadius, _config.MovableObstructionMask,
                    QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            var fire = _model.Ignite(destination, time, _config.SpreadInterval);
            var view = _views[destination];
            if (view == null)
            {
                _views[destination] = _instantiator.InstantiatePrefabForComponent<WaterFireTarget>(
                    _firePrefab, point, _firePrefab.transform.rotation, transform, new object[] { fire, _config });
            }
            else
            {
                view.Render();
            }

            return true;
        }
    }
}
