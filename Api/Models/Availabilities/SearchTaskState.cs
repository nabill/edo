using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public enum AvailabilitySearchTaskState
    {
        Unknown = 0,
        NotFound = 1,
        Failed = 2,
        Running = 3,
        Ready = 4
    }
}