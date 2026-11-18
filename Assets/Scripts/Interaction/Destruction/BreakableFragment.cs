using System.Collections;
using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class BreakableFragment : MonoBehaviour
    {
        private const int GroundMask = 1;
        private const float FloorSearchDistance = 12f;
        private const float FloorClearance = 0.008f;

        [SerializeField] private Renderer _surface;
        [SerializeField] private Collider _solid;
        [SerializeField, Min(0f)] private float _restHeight;

        private bool _detached;

        internal Collider Solid => _solid;

        internal static BreakableFragment FindFirstHit(Collider intact, BreakableFragment[] pieces, Ray ray, float reach)
        {
            intact.enabled = false;
            foreach (var piece in pieces)
            {
                piece.Solid.enabled = true;
            }

            Physics.SyncTransforms();
            BreakableFragment struck = null;
            foreach (var piece in pieces)
            {
                if (piece.Solid.Raycast(ray, out var hit, reach))
                {
                    struck = piece;
                    reach = hit.distance;
                }
            }

            if (struck != null)
            {
                return struck;
            }

            intact.enabled = true;
            foreach (var piece in pieces)
            {
                piece.Solid.enabled = false;
            }

            return null;
        }

        internal void Reveal()
        {
            _surface.enabled = true;
        }

        internal void Detach(Transform owner, Vector3 direction, InteractionConfigSO config)
        {
            if (_detached)
            {
                return;
            }

            _detached = true;
            if (_solid != null)
            {
                _solid.enabled = false;
            }

            gameObject.layer = config.DebrisLayer;
            Reveal();
            transform.SetParent(owner, true);
            StartCoroutine(Settle(direction, config.DebrisSettleSeconds, config.DebrisTravel));
        }

        private IEnumerator Settle(Vector3 direction, float seconds, float distance)
        {
            var start = transform.position;
            var startRotation = transform.rotation;
            var forward = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
            var side = Vector3.Cross(Vector3.up, forward);
            var scatter = Random.Range(-0.65f, 0.65f);
            var travel = (forward + side * scatter) * (distance * Random.Range(0.45f, 1f));
            var restHeight = _restHeight * transform.TransformVector(Vector3.up).magnitude;
            var end = FindLanding(start, travel, restHeight);
            var restRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            var elapsed = 0f;
            while (elapsed < seconds)
            {
                var progress = elapsed / seconds;
                var position = Vector3.Lerp(start, end, progress);
                position.y = Mathf.Lerp(start.y, end.y, progress * progress) + Mathf.Sin(progress * Mathf.PI) * 0.12f;
                transform.SetPositionAndRotation(position, Quaternion.Slerp(startRotation, restRotation, progress));
                yield return null;
                elapsed += Time.deltaTime;
            }

            transform.SetPositionAndRotation(end, restRotation);
        }

        private static Vector3 FindLanding(Vector3 start, Vector3 travel, float restHeight)
        {
            var distance = travel.magnitude;
            if (distance > 0f && Physics.Raycast(start, travel / distance, out var wall, distance,
                    GroundMask, QueryTriggerInteraction.Ignore))
            {
                travel = travel.normalized * Mathf.Max(0f, wall.distance - FloorClearance);
            }

            var end = start + travel;
            if (Physics.Raycast(end + Vector3.up * FloorClearance, Vector3.down, out var floor,
                    FloorSearchDistance, GroundMask, QueryTriggerInteraction.Ignore))
            {
                end.y = floor.point.y + restHeight + FloorClearance;
            }

            return end;
        }
    }
}
