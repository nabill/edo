using HappyTravel.Edo.Api.Models.Availabilities;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public readonly struct AvailabilitySearchState
    {
        [JsonConstructor]
        public AvailabilitySearchState(AvailabilitySearchTaskState taskState, int resultCount = 0, string error = null)
        {
            TaskState = taskState;
            ResultCount = resultCount;
            Error = error;
        }
        
        public AvailabilitySearchTaskState TaskState { get; }
        public int ResultCount { get; }
        public string Error { get; }
    }
}