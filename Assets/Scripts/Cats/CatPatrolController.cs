using FireSlop.Firefighter;
using FireSlop.Interaction;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace FireSlop.Cats
{
    [DefaultExecutionOrder(-20)]
    public sealed class CatPatrolController : MonoBehaviour
    {
        private const int QueryCapacity = 32;
        private static readonly ProfilerMarker PatrolMarker = new ProfilerMarker("Cat.Patrol");

        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private CatPatrolArea _area;

        private readonly Collider[] _nearby = new Collider[QueryCapacity];
        private readonly Vector3[] _corners = new Vector3[QueryCapacity];
        private NavMeshPath _path;
        private CatModel _cat;
        private HammerStunModel _hammerStun;
        private CatPatrolModel _patrol;
        private CatConfigSO _config;
        private MissionInputModel _input;
        private bool _started;
        private CatCarryable _carryable;

        [Inject]
        public void Construct(CatModel cat, CatPatrolModel patrol, CatConfigSO config, MissionInputModel input,
            HammerStunModel hammerStun)
        {
            _cat = cat;
            _hammerStun = hammerStun;
            _patrol = patrol;
            _config = config;
            _input = input;
        }

        private void Start()
        {
            _carryable = GetComponent<CatCarryable>();
            if (_carryable == null || _agent == null || _area == null || !_area.IsValid || !_area.Contains(transform.position))
            {
                Debug.LogError("Cat patrol requires a baked apartment area and an authored spawn inside it.", this);
                enabled = false;
                return;
            }

            _agent.speed = _config.RoamSpeed;
            _agent.acceleration = _config.RoamAcceleration;
            _agent.angularSpeed = _config.RoamTurnSpeed;
            _agent.stoppingDistance = _config.RoamArrivalDistance;
            _path = new NavMeshPath();
            _started = true;
            UpdateAgentState();
        }

        private void OnDisable()
        {
            if (_agent != null)
            {
                _agent.enabled = false;
            }
        }

        private void UpdateAgentState()
        {
            var wantsNavigation = _cat.State == CatState.Idle || _cat.State == CatState.Warning;
            _agent.updateRotation = _cat.State == CatState.Idle;
            if (!wantsNavigation)
            {
                _agent.enabled = false;
                return;
            }

            if (_agent.enabled)
            {
                if (!_agent.isOnNavMesh)
                {
                    _agent.enabled = false;
                }
                return;
            }

            var filter = new NavMeshQueryFilter
            {
                agentTypeID = _agent.agentTypeID,
                areaMask = _agent.areaMask
            };
            if (!_area.Contains(transform.position) ||
                !NavMesh.SamplePosition(transform.position, out _, _config.RoamObstacleRadius, filter))
            {
                return;
            }

            _carryable.PrepareForEncounter();
            _agent.enabled = true;
        }

        private void Update()
        {
            if (!_started)
            {
                return;
            }

            using (PatrolMarker.Auto())
            {
                TickPatrol();
            }
        }

        private void TickPatrol()
        {
            UpdateAgentState();
            if (!_agent.enabled || !_agent.isOnNavMesh)
            {
                _patrol.SetFireSafety(!IsSegmentSafe(transform.position, transform.position), _config.RoamPauseSeconds);
                return;
            }

            if (!_input.IsEnabled || _hammerStun.IsActive)
            {
                StopAgent();
                return;
            }

            _patrol.Advance(Time.deltaTime);
            if (_patrol.SafetySeconds <= 0f || !IsSegmentSafe(transform.position, transform.position))
            {
                UpdateFireSafety();
            }

            var canRoam = _cat.State == CatState.Idle && !_patrol.IsFireTrapped && _patrol.WaitSeconds <= 0f;
            if (canRoam)
            {
                _agent.isStopped = false;
            }
            else
            {
                StopAgent();
            }
            if (!canRoam || _agent.pathPending)
            {
                return;
            }

            if (_agent.hasPath && _agent.remainingDistance > _config.RoamArrivalDistance)
            {
                var direction = (_agent.steeringTarget - transform.position).normalized;
                var ahead = transform.position + direction * _config.RoamClearance;
                var fireSafe = IsSegmentSafe(transform.position, ahead);
                if (!fireSafe || !IsMovementClear(transform.position, ahead))
                {
                    _agent.ResetPath();
                    _patrol.NextPoint(_area.PointCount);
                    _patrol.SetFireSafety(!fireSafe, 0f);
                    _patrol.Wait(_config.RoamPauseSeconds);
                    StopAgent();
                }

                return;
            }

            _agent.ResetPath();
            ChooseDestination();
        }

        private void StopAgent()
        {
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
        }

        private void ChooseDestination()
        {
            for (var attempt = 0; attempt < _area.PointCount; attempt++)
            {
                _patrol.NextPoint(_area.PointCount);
                var destination = _area.GetPoint(_patrol.PointIndex);
                if (!_area.Contains(destination) || !_agent.CalculatePath(destination, _path) ||
                    _path.status != NavMeshPathStatus.PathComplete || !IsPathSafe() || !IsPathClear())
                {
                    continue;
                }

                _agent.SetPath(_path);
                _patrol.Wait(_config.RoamPauseSeconds);
                StopAgent();
                return;
            }

            _patrol.Wait(_config.RoamPauseSeconds);
        }

        private void UpdateFireSafety()
        {
            if (!IsSegmentSafe(transform.position, transform.position))
            {
                _patrol.SetFireSafety(true, _config.RoamPauseSeconds);
                return;
            }

            var blockedByFire = false;
            for (var index = 0; index < _area.PointCount; index++)
            {
                var destination = _area.GetPoint(index);
                if (Vector3.Distance(transform.position, destination) <= _config.RoamArrivalDistance)
                {
                    continue;
                }

                if (!_agent.CalculatePath(destination, _path) || _path.status != NavMeshPathStatus.PathComplete)
                {
                    continue;
                }

                if (IsPathSafe())
                {
                    _patrol.SetFireSafety(false, _config.RoamPauseSeconds);
                    return;
                }
                blockedByFire = true;
            }

            _patrol.SetFireSafety(blockedByFire, _config.RoamPauseSeconds);
        }

        private bool IsPathSafe()
        {
            var count = _path.GetCornersNonAlloc(_corners);
            if (count < 2 || count == _corners.Length)
            {
                return false;
            }

            for (var index = 1; index < count; index++)
            {
                if (!_area.Contains(_corners[index]) || !IsSegmentSafe(_corners[index - 1], _corners[index]))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsPathClear()
        {
            var count = _path.GetCornersNonAlloc(_corners);
            for (var index = 1; index < count; index++)
            {
                if (!IsMovementClear(_corners[index - 1], _corners[index]))
                {
                    return false;
                }
            }
            return count >= 2;
        }

        private bool IsMovementClear(Vector3 start, Vector3 end)
        {
            var travel = end - start;
            if (travel.sqrMagnitude <= 0f)
            {
                return true;
            }

            var radius = _config.RoamObstacleRadius;
            var bottom = start + Vector3.up * radius;
            var top = start + Vector3.up * (_agent.height - radius);
            return !Physics.CapsuleCast(bottom, top, radius, travel.normalized, travel.magnitude,
                _config.RoamObstacleMask, QueryTriggerInteraction.Ignore);
        }

        private bool IsSegmentSafe(Vector3 start, Vector3 end)
        {
            var lift = Vector3.up * _agent.height * 0.5f;
            var count = Physics.OverlapCapsuleNonAlloc(start + lift, end + lift, _config.RoamClearance,
                _nearby, _config.RoamFireMask, QueryTriggerInteraction.Collide);
            if (count == _nearby.Length)
            {
                return false;
            }

            for (var index = 0; index < count; index++)
            {
                if (_nearby[index].TryGetComponent<WaterFireTarget>(out var fire) && fire.RequestsAutoSpray)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
