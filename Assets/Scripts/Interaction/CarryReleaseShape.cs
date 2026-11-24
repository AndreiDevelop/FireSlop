using UnityEngine;

namespace FireSlop.Interaction
{
    // Stored relative to the carryable's pivot, in world units, before its collider is disabled.
    internal readonly struct CarryReleaseShape
    {
        private readonly Vector3 _center;
        private readonly Vector3 _halfAxis;
        private readonly float _radius;
        private readonly Vector3 _extents;
        private readonly Quaternion _orientation;

        private CarryReleaseShape(Vector3 center, Vector3 halfAxis, float radius, Vector3 extents, Quaternion orientation)
        {
            _center = center;
            _halfAxis = halfAxis;
            _radius = radius;
            _extents = extents;
            _orientation = orientation;
        }

        public static CarryReleaseShape Capture(Collider collider, Transform pivot)
        {
            var shape = collider.transform;
            var inverse = Quaternion.Inverse(pivot.rotation);
            var scale = shape.lossyScale;
            scale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            var orientation = inverse * shape.rotation;

            if (collider is SphereCollider sphere)
            {
                var center = inverse * (shape.TransformPoint(sphere.center) - pivot.position);
                return new CarryReleaseShape(center, Vector3.zero, sphere.radius * Mathf.Max(scale.x, Mathf.Max(scale.y, scale.z)),
                    Vector3.zero, orientation);
            }

            if (collider is CapsuleCollider capsule)
            {
                var center = inverse * (shape.TransformPoint(capsule.center) - pivot.position);
                var axis = Vector3.zero;
                var direction = capsule.direction;
                axis[direction] = 1f;
                var radius = capsule.radius * Mathf.Max(scale[(direction + 1) % 3], scale[(direction + 2) % 3]);
                var halfLength = Mathf.Max(0f, capsule.height * scale[direction] * 0.5f - radius);
                return new CarryReleaseShape(center, orientation * axis * halfLength, radius, Vector3.zero, orientation);
            }

            Bounds bounds;
            if (collider is BoxCollider box)
            {
                bounds = new Bounds(box.center, box.size);
            }
            else if (collider is MeshCollider mesh && mesh.sharedMesh != null)
            {
                bounds = mesh.sharedMesh.bounds;
            }
            else
            {
                bounds = collider.bounds;
                return new CarryReleaseShape(inverse * (bounds.center - pivot.position), Vector3.zero, 0f,
                    bounds.extents, inverse);
            }

            return new CarryReleaseShape(inverse * (shape.TransformPoint(bounds.center) - pivot.position),
                Vector3.zero, 0f, Vector3.Scale(bounds.extents, scale), orientation);
        }

        public bool TryPlace(Vector3 eye, Vector3 forward, float distance, LayerMask mask, out Vector3 position)
        {
            var rotation = Quaternion.LookRotation(forward);
            var orientation = rotation * _orientation;
            var axis = rotation * _halfAxis;
            var up = Quaternion.Inverse(orientation) * Vector3.up;
            var halfHeight = _radius > 0f ? _radius + Mathf.Abs(axis.y) :
                Mathf.Abs(up.x) * _extents.x + Mathf.Abs(up.y) * _extents.y + Mathf.Abs(up.z) * _extents.z;
            var origin = eye - Vector3.up * halfHeight;
            position = origin + forward * distance;
            return IsPathClear(origin, position, rotation, mask);
        }

        public bool IsPathClear(Vector3 origin, Vector3 position, Quaternion rotation, LayerMask mask)
        {
            var orientation = rotation * _orientation;
            var axis = rotation * _halfAxis;
            var center = origin + rotation * _center;
            var offset = position - origin;
            var distance = offset.magnitude;
            var forward = offset.normalized;
            const QueryTriggerInteraction triggers = QueryTriggerInteraction.Ignore;

            if (_radius == 0f)
            {
                return !Physics.BoxCast(center, _extents, forward, out _, orientation, distance, mask, triggers) &&
                    !Physics.CheckBox(center + offset, _extents, orientation, mask, triggers);
            }

            if (_halfAxis == Vector3.zero)
            {
                return !Physics.SphereCast(center, _radius, forward, out _, distance, mask, triggers) &&
                    !Physics.CheckSphere(center + offset, _radius, mask, triggers);
            }

            var top = center + axis;
            var bottom = center - axis;
            return !Physics.CapsuleCast(top, bottom, _radius, forward, distance, mask, triggers) &&
                !Physics.CheckCapsule(top + offset, bottom + offset, _radius, mask, triggers);
        }
    }
}
