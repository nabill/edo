using System;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct RoomContractSetAvailabilityInfo
    {
        private RoomContractSetAvailabilityInfo(
            string availabilityId,
            DateTime checkInDate,
            DateTime checkOutDate,
            int numberOfNights,
            in SlimAccommodation accommodation,
            in RoomContractSetInfo roomContractSet)
        {
            AvailabilityId = availabilityId;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            NumberOfNights = numberOfNights;
            Accommodation = accommodation;
            RoomContractSet = roomContractSet;
        }


        public static RoomContractSetAvailabilityInfo? FromRoomContractSetAvailability(in RoomContractSetAvailability? availability, Suppliers? supplier)
        {
            if (availability is null)
                return null;

            var availabilityValue = availability.Value;
            return new RoomContractSetAvailabilityInfo(availabilityValue.AvailabilityId,
                availabilityValue.CheckInDate,
                availabilityValue.CheckOutDate,
                availabilityValue.NumberOfNights,
                availabilityValue.Accommodation,
                RoomContractSetInfo.FromRoomContractSet(availabilityValue.RoomContractSet, supplier));
        }
        
        public string AvailabilityId { get; }
        public DateTime CheckInDate { get; }
        public DateTime CheckOutDate { get; }
        public int NumberOfNights { get; }
        public SlimAccommodation Accommodation { get; }
        public RoomContractSetInfo RoomContractSet { get; }
    }
}