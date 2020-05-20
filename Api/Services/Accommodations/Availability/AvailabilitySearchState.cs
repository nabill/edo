using System;
using HappyTravel.Edo.Api.Models.Availabilities;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public readonly struct AvailabilitySearchState
    {
        [JsonConstructor]
        private AvailabilitySearchState(Guid id, AvailabilitySearchTaskState taskState, int resultCount = 0, string error = null)
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


        public static AvailabilitySearchState Failed(Guid id, string error)
            => new AvailabilitySearchState(id, AvailabilitySearchTaskState.Failed, error: error);


        public static AvailabilitySearchState Completed(Guid id, int resultCount)
            => new AvailabilitySearchState(id, AvailabilitySearchTaskState.Completed, resultCount);


        public static AvailabilitySearchState PartiallyCompleted(Guid id, int resultCount)
            => new AvailabilitySearchState(id, AvailabilitySearchTaskState.PartiallyCompleted, resultCount);


        public static AvailabilitySearchState Pending(Guid id) => new AvailabilitySearchState(id, AvailabilitySearchTaskState.Pending);


        public static AvailabilitySearchState FromState(Guid id, AvailabilitySearchTaskState taskState, int resultCount)
            => new AvailabilitySearchState(id, taskState, resultCount);


        public bool Equals(AvailabilitySearchState other)
            => Id.Equals(other.Id) && TaskState == other.TaskState && ResultCount == other.ResultCount && Error == other.Error;


        public override bool Equals(object obj) => obj is AvailabilitySearchState other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Id, (int) TaskState, ResultCount, Error);
    }
}