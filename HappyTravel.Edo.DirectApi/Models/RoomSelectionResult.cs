using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HappyTravel.Edo.DirectApi.Models
{
    public readonly struct RoomSelectionResult
    {
        [JsonConstructor]
        public RoomSelectionResult(Guid searchId, string accommodationId, List<RoomContractSet> roomContractSets)
        {
            SearchId = searchId;
            AccommodationId = accommodationId;
            RoomContractSets = roomContractSets;
        }

        
        public Guid SearchId { get; }
        public string AccommodationId { get; }
        public List<RoomContractSet> RoomContractSets { get; }
    }
}