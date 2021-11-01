using System;
using System.Collections.Generic;

namespace HappyTravel.Edo.DirectApi.Models
{
    public readonly struct RoomSelectionResult
    {
        public RoomSelectionResult(Guid searchId, string htId, List<RoomContractSet> results)
        {
            SearchId = searchId;
            HtId = htId;
            Results = results;
        }

        
        public Guid SearchId { get; }
        public string HtId { get; }
        public List<RoomContractSet> Results { get; }
    }
}