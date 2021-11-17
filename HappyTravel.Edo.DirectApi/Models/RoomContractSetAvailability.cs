using System;
using System.Text.Json.Serialization;

namespace HappyTravel.Edo.DirectApi.Models
{
    public readonly struct RoomContractSetAvailability
    {
        [JsonConstructor]
        public RoomContractSetAvailability(Guid searchId, string accommodationId, RoomContractSet roomContractSet)
        {
            SearchId = searchId;
            AccommodationId = accommodationId;
            RoomContractSet = roomContractSet;
        }

        public Guid SearchId { get; }
        public string AccommodationId { get; }

        /// <summary>
        ///     Information about a selected room contract set.
        /// </summary>
        public RoomContractSet RoomContractSet { get; }
    }
}