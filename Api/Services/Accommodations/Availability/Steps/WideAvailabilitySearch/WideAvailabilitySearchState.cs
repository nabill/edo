using System;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.SuppliersCatalog;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public readonly struct WideAvailabilitySearchState
    {
        [JsonConstructor]
        private WideAvailabilitySearchState(Guid id, AvailabilitySearchTaskState taskState, int resultCount = 0, string error = null)
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


        public static WideAvailabilitySearchState FromSupplierStates(Guid searchId, IEnumerable<(Suppliers, SupplierAvailabilitySearchState)> searchStates)
        {
            var statesDictionary = searchStates
                .ToDictionary(s => s.Item1, s => s.Item2);
            
            var overallState = CalculateOverallState(statesDictionary);
            var totalResultsCount = GetResultsCount(statesDictionary);
            var errors = GetErrors(statesDictionary);
            
            return new WideAvailabilitySearchState(searchId, overallState, totalResultsCount, errors);


            static AvailabilitySearchTaskState CalculateOverallState(Dictionary<Suppliers, SupplierAvailabilitySearchState> supplierSearchStates)
            {
                var searchStates = supplierSearchStates
                    .Select(r => r.Value.TaskState)
                    .ToHashSet();

                if (searchStates.Count == 1)
                    return searchStates.Single();

                if (searchStates.Contains(AvailabilitySearchTaskState.Pending))
                    return AvailabilitySearchTaskState.PartiallyCompleted;

                if (searchStates.All(s => s == AvailabilitySearchTaskState.Completed || s == AvailabilitySearchTaskState.Failed))
                    return AvailabilitySearchTaskState.Completed;
                
                throw new ArgumentException($"Invalid tasks state: {string.Join(";", searchStates)}");
            }


            static string GetErrors(Dictionary<Suppliers, SupplierAvailabilitySearchState> states)
            {
                var errors = states
                    .Select(p => p.Value.Error)
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .ToArray();

                return string.Join("; ", errors);
            }


            static int GetResultsCount(Dictionary<Suppliers, SupplierAvailabilitySearchState> states)
            {
                var totalCount = states.Sum(s => s.Value.ResultCount);
                var duplicates = 0;

                if (states.All(s => s.Value.TaskState == AvailabilitySearchTaskState.Completed))
                {
                    duplicates = states
                        .SelectMany(state => state.Value.HtIds.Select(id => id))
                        .Where(x => !string.IsNullOrEmpty(x))
                        .GroupBy(x => x)
                        .Select(x => new { Count = x.Count() - 1 })
                        .Where(x => x.Count > 0)
                        .Sum(x => x.Count);
                }

                return totalCount - duplicates;
            }
        }


        public bool Equals(WideAvailabilitySearchState other)
            => Id.Equals(other.Id) && TaskState == other.TaskState && ResultCount == other.ResultCount && Error == other.Error;


        public override bool Equals(object obj) => obj is WideAvailabilitySearchState other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Id, (int) TaskState, ResultCount, Error);
    }
}