using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace FireSlop.Interaction
{
    internal sealed class FireFloorTopology
    {
        private const int InitialOverlapCapacity = 8;
        private readonly Vector3[] _positions;

        private FireFloorTopology(Vector3[] positions, FireSpreadLink[] links, FireSpreadLink[] doorLinks,
            int seedIndex, int additionalSeedIndex)
        {
            _positions = positions;
            Links = links;
            DoorLinks = doorLinks;
            SeedIndex = seedIndex;
            AdditionalSeedIndex = additionalSeedIndex;
        }

        public int CellCount => _positions.Length;
        public int SeedIndex { get; }
        public int AdditionalSeedIndex { get; }
        public IReadOnlyList<FireSpreadLink> Links { get; }
        public IReadOnlyList<FireSpreadLink> DoorLinks { get; }

        public Vector3 PositionAt(int index)
        {
            return _positions[index];
        }

        public static FireFloorTopology Build(Vector3 seed, Vector3? additionalSeed, Collider[] floors,
            FireSpreadConfigSO config)
        {
            var bounds = floors[0].bounds;
            foreach (var floor in floors)
            {
                bounds.Encapsulate(floor.bounds);
            }

            var spacing = config.CellSpacing;
            var minX = Mathf.FloorToInt((bounds.min.x - seed.x) / spacing);
            var maxX = Mathf.CeilToInt((bounds.max.x - seed.x) / spacing);
            var minZ = Mathf.CeilToInt((bounds.min.z - seed.z) / spacing);
            var maxZ = Mathf.FloorToInt((bounds.max.z - seed.z) / spacing);
            var cells = new Dictionary<Vector2Int, int>();
            var positions = new List<Vector3>();
            for (var x = minX; x <= maxX; x++)
            {
                var offsetX = GetColumnOffset(x, spacing, config.CorridorLaneSpacing);
                for (var z = minZ; z <= maxZ; z++)
                {
                    var point = seed + new Vector3(offsetX, 0f, z * spacing);
                    if (!TryGetFloor(point, floors, bounds, out var position))
                    {
                        continue;
                    }

                    cells.Add(new Vector2Int(x, z), positions.Count);
                    positions.Add(position);
                }
            }

            if (!cells.TryGetValue(Vector2Int.zero, out var seedIndex))
            {
                return null;
            }

            var additionalSeedIndex = -1;
            if (additionalSeed.HasValue)
            {
                if (!TryGetFloor(additionalSeed.Value, floors, bounds, out var position))
                {
                    return null;
                }

                additionalSeedIndex = FindClosestCell(positions, position, spacing);
                if (additionalSeedIndex < 0 || additionalSeedIndex == seedIndex)
                {
                    return null;
                }

                positions[additionalSeedIndex] = position;
            }

            var links = new List<FireSpreadLink>(cells.Count * 2);
            var overlaps = new Collider[InitialOverlapCapacity];
            foreach (var cell in cells)
            {
                AddLink(cell, Vector2Int.right, cells, positions, floors, bounds, config, links, ref overlaps);
                AddLink(cell, Vector2Int.up, cells, positions, floors, bounds, config, links, ref overlaps);
            }

            // Keep the checked routes through doorways and breaches, and scatter their ignition points.
            var spreadPositions = positions.ToArray();
            if (config.PatchOffsetFraction > 0f)
            {
                var random = new Random();
                for (var index = 0; index < spreadPositions.Length; index++)
                {
                    if (index != seedIndex && index != additionalSeedIndex)
                    {
                        spreadPositions[index] = ScatterPatch(positions[index], floors, bounds, config, random);
                    }
                }
            }

            var doorLinks = links.FindAll(link => link.HasDoor);
            return new FireFloorTopology(spreadPositions, links.ToArray(), doorLinks.ToArray(),
                seedIndex, additionalSeedIndex);
        }

        private static int FindClosestCell(List<Vector3> positions, Vector3 position, float spacing)
        {
            var closest = -1;
            var maxSquaredDistance = spacing * spacing * 0.5f;
            for (var index = 0; index < positions.Count; index++)
            {
                var squaredDistance = (positions[index] - position).sqrMagnitude;
                if (squaredDistance < maxSquaredDistance)
                {
                    maxSquaredDistance = squaredDistance;
                    closest = index;
                }
            }

            return closest;
        }

        private static Vector3 ScatterPatch(Vector3 anchor, Collider[] floors, Bounds bounds,
            FireSpreadConfigSO config, Random random)
        {
            const int maxAttempts = 8;
            var height = Vector3.up * config.ClearanceHeight;
            var radius = config.ClearanceRadius;
            var maxX = config.CorridorLaneSpacing * config.PatchOffsetFraction;
            var maxZ = config.CellSpacing * config.PatchOffsetFraction;

            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                var x = (float)(2d * random.NextDouble() - 1d) * maxX;
                var z = (float)(2d * random.NextDouble() - 1d) * maxZ;
                var point = anchor + new Vector3(x, 0f, z);
                if (!TryGetFloor(point, floors, bounds, out var position))
                {
                    continue;
                }

                var center = position + height;
                if (!Physics.CheckSphere(center, radius, config.StructureMask, QueryTriggerInteraction.Ignore) &&
                    !Physics.Linecast(anchor + height, center, config.StructureMask, QueryTriggerInteraction.Ignore))
                {
                    return position;
                }
            }

            return anchor;
        }

        private static float GetColumnOffset(int column, float cellSpacing, float laneSpacing)
        {
            if (column == 0)
            {
                return 0f;
            }

            // The three corridor lanes share the seed's centerline. Rooms keep the regular spacing.
            var distance = laneSpacing + (Mathf.Abs(column) - 1) * cellSpacing;
            return column > 0 ? distance : -distance;
        }

        private static bool TryGetFloor(Vector3 point, Collider[] floors, Bounds bounds, out Vector3 position)
        {
            var origin = new Vector3(point.x, bounds.max.y + 1f, point.z);
            var ray = new Ray(origin, Vector3.down);
            var distance = bounds.size.y + 1f;
            foreach (var floor in floors)
            {
                if (floor.Raycast(ray, out var hit, distance))
                {
                    position = hit.point;
                    return true;
                }
            }

            position = default;
            return false;
        }

        private static void AddLink(KeyValuePair<Vector2Int, int> cell, Vector2Int direction,
            Dictionary<Vector2Int, int> cells, List<Vector3> positions, Collider[] floors, Bounds bounds,
            FireSpreadConfigSO config, List<FireSpreadLink> links, ref Collider[] overlaps)
        {
            if (!cells.TryGetValue(cell.Key + direction, out var adjacent))
            {
                return;
            }

            var start = positions[cell.Value];
            var end = positions[adjacent];
            if (!TryGetFloor((start + end) * 0.5f, floors, bounds, out _))
            {
                return;
            }

            var height = Vector3.up * config.ClearanceHeight;
            var hitCount = FindObstructions(start + height, end + height, config, ref overlaps);
            List<DoorView> doors = null;
            List<Collider> bricks = null;
            for (var index = 0; index < hitCount; index++)
            {
                var hit = overlaps[index];
                var door = hit.GetComponentInParent<DoorView>();
                if (door != null)
                {
                    doors ??= new List<DoorView>();
                    if (!doors.Contains(door))
                    {
                        doors.Add(door);
                    }
                }
                else if (hit.GetComponentInParent<BreakableBrickView>() != null)
                {
                    bricks ??= new List<Collider>();
                    bricks.Add(hit);
                }
                else
                {
                    return;
                }
            }

            links.Add(new FireSpreadLink(cell.Value, adjacent,
                doors?.ToArray() ?? Array.Empty<DoorView>(), bricks?.ToArray() ?? Array.Empty<Collider>()));
        }

        private static int FindObstructions(Vector3 start, Vector3 end, FireSpreadConfigSO config,
            ref Collider[] overlaps)
        {
            var count = Physics.OverlapCapsuleNonAlloc(start, end, config.ClearanceRadius, overlaps,
                config.StructureMask, QueryTriggerInteraction.Ignore);
            // A full buffer may omit a solid collider that makes the link impassable.
            while (count == overlaps.Length)
            {
                overlaps = new Collider[overlaps.Length * 2];
                count = Physics.OverlapCapsuleNonAlloc(start, end, config.ClearanceRadius, overlaps,
                    config.StructureMask, QueryTriggerInteraction.Ignore);
            }

            return count;
        }
    }
}
