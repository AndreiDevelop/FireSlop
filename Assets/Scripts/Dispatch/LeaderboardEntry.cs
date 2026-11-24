namespace FireSlop.Dispatch
{
    public sealed class LeaderboardEntry
    {
        public LeaderboardEntry(string name, int score, int avatarIndex)
        {
            Name = name;
            Score = score;
            AvatarIndex = avatarIndex;
        }

        public string Name { get; }
        public int Score { get; }
        public int AvatarIndex { get; }
        public bool IsPlayer => AvatarIndex < 0;
    }
}
