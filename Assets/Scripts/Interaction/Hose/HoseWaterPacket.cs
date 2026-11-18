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
            EmissionEndOrigin = origin;
            EmissionEndVelocity = launchVelocity;
            EmissionDuration = 0f;
            IsEmissionSealed = false;
            AffectsTargets = true;
            NozzleObstructed = false;
        }

        public Vector3 Origin { get; }
        public Vector3 LaunchVelocity { get; }
        public double LaunchTime { get; }
        public float FlightLimit { get; set; }
        public float CheckedAge { readonly get; set; }
        public float ConfirmedAge { get; set; }
        public float SegmentStartAge { readonly get; set; }
        public bool HasPendingImpact { get; set; }
        public bool HasImpacted { get; set; }
        public int DropletCount { get; set; }
        public Vector3 EmissionEndOrigin { get; private set; }
        public Vector3 EmissionEndVelocity { get; private set; }
        public float EmissionDuration { get; private set; }
        public bool IsEmissionSealed { get; private set; }
        public bool AffectsTargets { get; set; }
        public bool NozzleObstructed { get; set; }

        public void UpdateEmissionEnd(Vector3 origin, Vector3 velocity, double time, bool seal)
        {
            if (IsEmissionSealed)
            {
                return;
            }

            EmissionEndOrigin = NozzleObstructed ? Origin : origin;
            EmissionEndVelocity = velocity;
            EmissionDuration = Mathf.Max(0f, (float)(time - LaunchTime));
            IsEmissionSealed = seal;
        }

        public readonly Vector3 PositionAt(float age, Vector3 gravity)
        {
            return Origin + (LaunchVelocity + gravity * (0.5f * age)) * age;
        }

        public Vector3 VelocityAt(float age, Vector3 gravity)
        {
            return LaunchVelocity + gravity * age;
        }
    }
}
