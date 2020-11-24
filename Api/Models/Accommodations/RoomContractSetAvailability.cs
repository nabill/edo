using System;
using HappyTravel.EdoContracts.Accommodations.Internals;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct RoomContractSetAvailability
    {
        public RoomContractSetAvailability(string availabilityId, DateTime checkInDate, DateTime checkOutDate, int numberOfNights,
            in SlimAccommodation accommodation, in RoomContractSet roomContractSet)
        {
            AvailabilityId = availabilityId;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            NumberOfNights = numberOfNights;
            Accommodation = accommodation;
            RoomContractSet = roomContractSet;
        }
        
        public string AvailabilityId { get; }
        public DateTime CheckInDate { get; }
        public DateTime CheckOutDate { get; }
        public int NumberOfNights { get; }
        public SlimAccommodation Accommodation { get; }
        public RoomContractSet RoomContractSet { get; }
    }
}