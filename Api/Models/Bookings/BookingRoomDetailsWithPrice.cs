using System.Collections.Generic;
using HappyTravel.EdoContracts.General;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    public struct BookingRoomDetailsWithPrice
    {
        [JsonConstructor]
        public BookingRoomDetailsWithPrice(BookingRoomDetails roomDetails, List<Price> prices)
        {
            RoomDetails = roomDetails;
            Prices = prices;
        }


        public BookingRoomDetails RoomDetails { get; }
        public List<Price> Prices { get; }
    }
}