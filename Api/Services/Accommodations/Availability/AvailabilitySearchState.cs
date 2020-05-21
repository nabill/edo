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


        /// <summary>
        /// Search id
        /// </summary>
        public Guid Id { get; }
        
        /// <summary>
        /// Search task state
        /// </summary>
        public AvailabilitySearchTaskState TaskState { get; }
        
        /// <summary>
        /// Result count
        /// </summary>
        public int ResultCount { get; }
        
        /// <summary>
        /// Error message. Filled only for failed tasks
        /// </summary>
        public string Error { get; }


        public static AvailabilitySearchState Failed(Guid id, string error)
            => new AvailabilitySearchState(id, AvailabilitySearchTaskState.Failed, error: error);


        public static AvailabilitySearchState Completed(Guid id, int resultCount, string error = null)
            => new AvailabilitySearchState(id, AvailabilitySearchTaskState.Completed, resultCount, error);


        public static AvailabilitySearchState PartiallyCompleted(Guid id, int resultCount, string error)
            => new AvailabilitySearchState(id, AvailabilitySearchTaskState.PartiallyCompleted, resultCount);


        public static AvailabilitySearchState Pending(Guid id) => new AvailabilitySearchState(id, AvailabilitySearchTaskState.Pending);


        public static AvailabilitySearchState FromState(Guid id, AvailabilitySearchTaskState taskState, int resultCount, string error)
            => new AvailabilitySearchState(id, taskState, resultCount, error);


        public bool Equals(AvailabilitySearchState other)
            => Id.Equals(other.Id) && TaskState == other.TaskState && ResultCount == other.ResultCount && Error == other.Error;


        public override bool Equals(object obj) => obj is AvailabilitySearchState other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Id, (int) TaskState, ResultCount, Error);
    }
}