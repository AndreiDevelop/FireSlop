using UnityEngine;

namespace FireSlop.Interaction
{
    internal readonly struct FireSpreadLink
    {
        private readonly DoorView[] _doors;
        private readonly Collider[] _bricks;

        public FireSpreadLink(int first, int second, DoorView[] doors, Collider[] bricks)
        {
            First = first;
            Second = second;
            _doors = doors;
            _bricks = bricks;
        }

        public int First { get; }
        public int Second { get; }
        public bool HasDoor => _doors.Length > 0;

        public bool HasBrokenDoor()
        {
            foreach (var door in _doors)
            {
                if (door != null && door.Door.IsBroken)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsOpen()
        {
            foreach (var door in _doors)
            {
                if (door != null && !door.Door.IsPassable)
                {
                    return false;
                }
            }

            foreach (var brick in _bricks)
            {
                if (brick != null && brick.enabled && brick.gameObject.activeInHierarchy)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
