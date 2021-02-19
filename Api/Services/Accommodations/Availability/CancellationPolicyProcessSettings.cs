using System;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public readonly struct CancellationPolicyProcessSettings
    {
        public TimeSpan PolicyStartDateShift { get; init; }
    }
}