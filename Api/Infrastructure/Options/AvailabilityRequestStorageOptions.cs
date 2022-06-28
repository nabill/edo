using System;

namespace HappyTravel.Edo.Api.Infrastructure.Options;

public class AvailabilityRequestStorageOptions
{
    public TimeSpan StorageLifeTime { get; set; }
}