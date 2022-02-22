using System;
using System.Text.Json.Serialization;

namespace HappyTravel.Edo.DirectApi.Models.Search
{
    public readonly struct RoomContractSetAvailability
    {
        [JsonConstructor]
        public RoomContractSetAvailability(Guid searchId, string accommodationId, RoomContractSet? roomContractSet)
        {
            SearchId = searchId;
            AccommodationId = accommodationId;
            RoomContractSet = roomContractSet;
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
        ///     Information about a selected room contract set
        /// </summary>
        public RoomContractSet? RoomContractSet { get; }
    }
}