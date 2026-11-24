using System;
using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class BallisticTrajectory
    {
        public const int MaxPoints = 81;
        public const float Step = 0.05f;

        private readonly Vector3[] _points = new Vector3[MaxPoints];
        private readonly RaycastHit[] _hits = new RaycastHit[32];

        public int PointCount { get; private set; }
        public bool IsOriginClear { get; private set; }
        public bool HasContact { get; private set; }
        public RaycastHit Contact { get; private set; }
        public float Duration => (PointCount - 1) * Step;

        public void Trace(Vector3 eye, Vector3 start, Vector3 velocity, float radius, Transform player,
            Transform ignored)
        {
            HasContact = false;
            Contact = default;
            IsOriginClear = !FindContact(eye, start, radius, player, ignored, out var obstruction);
            if (!IsOriginClear)
            {
                _points[0] = eye;
                _points[1] = obstruction.point;
                PointCount = 2;
                Contact = obstruction;
                HasContact = true;
                return;
            }

            _points[0] = start;
            PointCount = MaxPoints;
            for (var i = 1; i < MaxPoints; i++)
            {
                var time = i * Step;
                var next = start + velocity * time + Physics.gravity * (0.5f * time * time);
                if (FindContact(_points[i - 1], next, radius, player, ignored, out var hit))
                {
                    _points[i] = hit.point + hit.normal * radius;
                    PointCount = i + 1;
                    Contact = hit;
                    HasContact = true;
                    return;
                }

                _points[i] = next;
            }
        }

        public void CopyPositions(Vector3[] destination)
        {
            Array.Copy(_points, destination, PointCount);
        }

        public Vector3 PositionAt(float seconds)
        {
            var sample = Mathf.Clamp(seconds / Step, 0f, PointCount - 1);
            var index = Mathf.FloorToInt(sample);
            if (index >= PointCount - 1)
            {
                return _points[PointCount - 1];
            }

            return Vector3.Lerp(_points[index], _points[index + 1], sample - index);
        }

        private bool FindContact(Vector3 start, Vector3 end, float radius, Transform player, Transform ignored,
            out RaycastHit contact)
        {
            var delta = end - start;
            var hits = Physics.SphereCastNonAlloc(start, radius, delta.normalized, _hits, delta.magnitude,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            var distance = float.PositiveInfinity;
            contact = default;
            for (var i = 0; i < hits; i++)
            {
                var hit = _hits[i];
                var belongsToPlayer = hit.collider.transform.IsChildOf(player);
                var belongsToItem = ignored != null && hit.collider.transform.IsChildOf(ignored);
                if (hit.distance < distance && !belongsToPlayer && !belongsToItem)
                {
                    contact = hit;
                    distance = hit.distance;
                }
            }

            return distance < float.PositiveInfinity;
        }
    }
}
