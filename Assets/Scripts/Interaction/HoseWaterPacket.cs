using UnityEngine;

namespace FireSlop.Interaction
{
    internal struct HoseWaterPacket
    {
        public HoseWaterPacket(Vector3 origin, Vector3 launchVelocity, double launchTime, float flightLimit)
        {
            Origin = origin;
            LaunchVelocity = launchVelocity;
            LaunchTime = launchTime;
            FlightLimit = flightLimit;
            CheckedAge = 0f;
            ConfirmedAge = 0f;
            SegmentStartAge = 0f;
            HasPendingImpact = false;
            HasImpacted = false;
            DropletCount = 0;
        }

        public Vector3 Origin { get; }
        public Vector3 LaunchVelocity { get; }
        public double LaunchTime { get; }
        public float FlightLimit { get; set; }
        public float CheckedAge { get; set; }
        public float ConfirmedAge { get; set; }
        public float SegmentStartAge { get; set; }
        public bool HasPendingImpact { get; set; }
        public bool HasImpacted { get; set; }
        public int DropletCount { get; set; }

        public Vector3 PositionAt(float age, Vector3 gravity)
        {
            return Origin + (LaunchVelocity + gravity * (0.5f * age)) * age;
        }

        public Vector3 VelocityAt(float age, Vector3 gravity)
        {
            return LaunchVelocity + gravity * age;
        }
    }
}
