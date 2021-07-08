using System;
using System.Collections.Generic;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public readonly struct SupplierAvailabilitySearchState
    {
        [JsonConstructor]
        private SupplierAvailabilitySearchState(Guid id, AvailabilitySearchTaskState taskState, List<string> htIds, int resultCount = 0, string error = null)
        {
            Id = id;
            TaskState = taskState;
            ResultCount = resultCount;
            Error = error;
            HtIds = htIds;
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

        /// <summary>
        /// Duplicate reports ids
        /// </summary>
        public List<string> HtIds { get; }


        public static SupplierAvailabilitySearchState Failed(Guid id, string error)
            => new SupplierAvailabilitySearchState(id, AvailabilitySearchTaskState.Failed, new List<string>(), error: error);


        public static SupplierAvailabilitySearchState Completed(Guid id, List<string> htIds, int resultCount, string error = null)
            => new SupplierAvailabilitySearchState(id, AvailabilitySearchTaskState.Completed, htIds, resultCount, error);

        
        public static SupplierAvailabilitySearchState Pending(Guid id) => new SupplierAvailabilitySearchState(id, AvailabilitySearchTaskState.Pending, new List<string>());


        public bool Equals(WideAvailabilitySearchState other)
            => Id.Equals(other.Id) && TaskState == other.TaskState && ResultCount == other.ResultCount && Error == other.Error;


        public override bool Equals(object obj) => obj is WideAvailabilitySearchState other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Id, (int) TaskState, ResultCount, Error);
    }
}