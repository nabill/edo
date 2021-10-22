using System;
using System.Collections.Generic;

namespace HappyTravel.Edo.DirectApi.Models
{
    public readonly struct WideSearchResult
    {
        public WideSearchResult(Guid searchId, bool isComplete, List<WideAvailabilityResult> results)
        {
            SearchId = searchId;
            IsComplete = isComplete;
            Results = results;
        }

        
        public Guid SearchId { get; }
        public bool IsComplete { get; }
        public List<WideAvailabilityResult> Results { get; }
    }
}