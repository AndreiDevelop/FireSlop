using UnityEngine;

namespace FireSlop.MissionHud
{
    public sealed class QuestTargetModel
    {
        public QuestTargetModel(Vector3 position)
        {
            Position = position;
        }

        public Vector3 Position { get; }
    }
}
