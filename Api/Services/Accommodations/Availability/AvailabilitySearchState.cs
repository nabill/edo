using System;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public readonly struct AvailabilitySearchState
    {
        [JsonConstructor]
        public AvailabilitySearchState(Guid id, AvailabilitySearchTaskState taskState, int resultCount = 0, string error = null)
        {
            Id = id;
            TaskState = taskState;
            ResultCount = resultCount;
            Error = error;
        }

        public Guid Id { get; }
        public AvailabilitySearchTaskState TaskState { get; }
        public int ResultCount { get; }
        public string Error { get; }
    }
}