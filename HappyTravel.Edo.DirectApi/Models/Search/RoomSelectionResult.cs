using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HappyTravel.Edo.DirectApi.Models.Search
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

        
        /// <summary>
        ///     ID for the search
        /// </summary>
        public Guid SearchId { get; }

        /// <summary>
        ///     ID for the accommodation
        /// </summary>
        public string AccommodationId { get; }

        /// <summary>
        ///     Information about selected room contract sets
        /// </summary>
        public List<RoomContractSet> RoomContractSets { get; }
    }
}