using System.Collections.Generic;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.General;
using Newtonsoft.Json;

namespace HappyTravel.Edo.DirectApi.Models
{
    public readonly struct BookingRoomDetails
    {
        [JsonConstructor]
        public BookingRoomDetails(RoomTypes type, List<Pax> passengers, bool isExtraBedNeeded = false, bool isCotNeeded = false)
        {
            Type = type;
            Passengers = passengers;
            IsExtraBedNeeded = isExtraBedNeeded;
            IsCotNeededNeeded = isCotNeeded;
        }


        public List<Pax> Passengers { get; }

        public RoomTypes Type { get; }

        /// <summary>
        ///     Indicates if extra child bed needed.
        /// </summary>
        public bool IsExtraBedNeeded { get; }

        /// <summary>
        ///     Indicates if extra cot needed.
        /// </summary>
        public bool IsCotNeededNeeded { get; }
    }
}