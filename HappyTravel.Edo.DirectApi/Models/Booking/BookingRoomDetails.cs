using System.Collections.Generic;
using System.Text.Json.Serialization;
using HappyTravel.EdoContracts.Accommodations.Enums;

namespace HappyTravel.Edo.DirectApi.Models.Booking
{
    public readonly struct BookingRoomDetails
    {
        [JsonConstructor]
        public BookingRoomDetails(RoomTypes? type, List<Pax> passengers)
        {
            Type = type;
            Passengers = passengers;
        }


        public List<Pax> Passengers { get; }

        public RoomTypes? Type { get; }
    }
}