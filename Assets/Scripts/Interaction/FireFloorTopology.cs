using System;
using System.Collections.Generic;
using UnityEngine;

namespace FireSlop.Interaction
{
    internal sealed class FireFloorTopology
    {
        private readonly Vector3[] _positions;

        private FireFloorTopology(Vector3[] positions, FireSpreadLink[] links, int seedIndex)
        {
            _positions = positions;
            Links = links;
            SeedIndex = seedIndex;
        }

        public int CellCount => _positions.Length;
        public int SeedIndex { get; }
        public IReadOnlyList<FireSpreadLink> Links { get; }

        public Vector3 PositionAt(int index)
        {
            return _positions[index];
        }

        public static FireFloorTopology Build(Vector3 seed, Collider[] floors, FireSpreadConfigSO config)
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

            var links = new List<FireSpreadLink>(cells.Count * 2);
            foreach (var cell in cells)
            {
                AddLink(cell, Vector2Int.right, cells, positions, floors, bounds, config, links);
                AddLink(cell, Vector2Int.up, cells, positions, floors, bounds, config, links);
            }

            return new FireFloorTopology(positions.ToArray(), links.ToArray(), seedIndex);
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
            FireSpreadConfigSO config, List<FireSpreadLink> links)
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
            var hits = Physics.OverlapCapsule(start + height, end + height, config.ClearanceRadius,
                config.StructureMask, QueryTriggerInteraction.Ignore);
            List<DoorView> doors = null;
            List<Collider> bricks = null;
            foreach (var hit in hits)
            {
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
    }
}
