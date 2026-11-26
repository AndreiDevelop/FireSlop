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
            TracksNearContact = false;
            SampleDuration = 0f;
            ContactPoint = Vector3.zero;
            ContactNormal = Vector3.zero;
            EmissionEndOrigin = origin;
            EmissionEndVelocity = launchVelocity;
            EmissionDuration = 0f;
            IsEmissionSealed = false;
            AffectsTargets = true;
            CanPush = false;
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
        public float SampleDuration { get; set; }
        public bool TracksNearContact { get; set; }
        public Vector3 ContactPoint { get; private set; }
        public Vector3 ContactNormal { get; private set; }

        public Vector3 EmissionEndOrigin { get; private set; }
        public Vector3 EmissionEndVelocity { get; private set; }
        public float EmissionDuration { get; private set; }
        public bool IsEmissionSealed { get; private set; }
        public bool AffectsTargets { get; set; }
        public bool CanPush { get; set; }
        public bool NozzleObstructed { get; set; }

        public void SetContact(RaycastHit hit)
        {
            ContactPoint = hit.point;
            ContactNormal = hit.normal;
        }

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

        public readonly Vector3 StreamPosition(Vector3 gravity, float age, float flightAge)
        {
            var blend = Mathf.Clamp01((age - flightAge) / EmissionDuration);
            var hasContact = HasImpacted || HasPendingImpact;
            if (hasContact && !TracksNearContact)
            {
                var impactWindow = Mathf.Min(EmissionDuration, FlightLimit);
                blend *= Mathf.Clamp01((FlightLimit - flightAge) / impactWindow);
            }

            GetEmissionPose(blend, out var origin, out var velocity);
            var position = origin + (velocity + gravity * (0.5f * flightAge)) * flightAge;
            if (TracksNearContact && hasContact && FlightLimit > 0f)
            {
                // A sphere cast stops the stream centre short of the surface. Finish the visible arc
                // on the contact plane while allowing the arriving water to move along that plane.
                var end = origin + (velocity + gravity * (0.5f * FlightLimit)) * FlightLimit;
                var offset = Vector3.Dot(ContactPoint - end, ContactNormal);
                position += ContactNormal * (offset * Mathf.SmoothStep(0f, 1f, flightAge / FlightLimit));
            }

            return position;
        }

        public readonly void GetEmissionPose(float blend, out Vector3 origin, out Vector3 velocity)
        {
            origin = Vector3.Lerp(Origin, EmissionEndOrigin, blend);
            velocity = Vector3.Slerp(LaunchVelocity, EmissionEndVelocity, blend);
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
