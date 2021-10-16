using System;
using System.Collections.Generic;

namespace HappyTravel.Edo.DirectApi.Models
{
    public readonly struct RoomSelectionResult
    {
        public RoomSelectionResult(Guid searchId, string htId, bool isComplete, List<RoomContractSet> results)
        {
            SearchId = searchId;
            HtId = htId;
            IsComplete = isComplete;
            Results = results;
        }

        
        public Guid SearchId { get; }
        public string HtId { get; }
        public bool IsComplete { get; }
        public List<RoomContractSet> Results { get; }
    }
}