using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HappyTravel.Edo.DirectApi.Models.Search
{
    public readonly struct WideSearchResult
    {
        [JsonConstructor]
        public WideSearchResult(Guid searchId, bool isComplete, List<WideAvailabilityResult> accommodations)
        {
            SearchId = searchId;
            IsComplete = isComplete;
            Accommodations = accommodations;
        }

        
        public Guid SearchId { get; }
        public bool IsComplete { get; }
        public List<WideAvailabilityResult> Accommodations { get; }
    }
}