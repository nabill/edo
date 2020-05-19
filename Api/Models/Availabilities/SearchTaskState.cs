using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Api.Models.Availabilities
{
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public enum AvailabilitySearchTaskState
    {
        Unknown = 0,
        NotFound = 1,
        Failed = 2,
        Pending = 3,
        PartiallyCompleted = 4,
        Completed = 5
    }
}