using System.Collections.Generic;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.General;

namespace HappyTravel.Edo.Data.Booking
{
    public readonly struct BookedRoom
    {
        public BookedRoom(RoomTypes type, List<Pax> passengers, bool isExtraBedNeeded, List<Price> prices)
        {
            Type = type;
            Passengers = passengers;
            IsExtraBedNeeded = isExtraBedNeeded;
            Prices = prices;
        }


        public RoomTypes Type { get; }
        public List<Pax> Passengers { get; }
        public bool IsExtraBedNeeded { get; }
        public List<Price> Prices { get; }
    }
}