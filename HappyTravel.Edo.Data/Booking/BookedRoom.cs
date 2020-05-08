using System.Collections.Generic;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.General;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Data.Booking
{
    public readonly struct BookedRoom
    {
        public BookedRoom(RoomTypes type, List<Pax> passengers, bool isExtraBedNeeded, MoneyAmount price)
        {
            Type = type;
            Passengers = passengers;
            IsExtraBedNeeded = isExtraBedNeeded;
            Price = price;
        }


        public RoomTypes Type { get; }
        public List<Pax> Passengers { get; }
        public bool IsExtraBedNeeded { get; }
        public MoneyAmount Price { get; }
    }
}