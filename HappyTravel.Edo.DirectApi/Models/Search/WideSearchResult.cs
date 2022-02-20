using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HappyTravel.Edo.DirectApi.Models.Search
{
    // TODO: WideAvailabilityResult and WideSearchResult names are quite similar
    public readonly struct WideSearchResult
    {
        [JsonConstructor]
        public WideSearchResult(Guid searchId, bool isComplete, List<WideAvailabilityResult> accommodations)
        {
            SearchId = searchId;
            IsComplete = isComplete;
            Accommodations = accommodations;
        }

        
        /// <summary>
        ///     ID for the search
        /// </summary>
        public Guid SearchId { get; }

        /// <summary>
        ///     Indicates if the search complete
        /// </summary>
        public bool IsComplete { get; }

        /// <summary>
        ///     List of available accommodations
        /// </summary>
        public List<WideAvailabilityResult> Accommodations { get; }
    }
}