using System;
using System.Collections.Generic;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;

namespace HappyTravel.Edo.Api.Models.Mailing
{
    public readonly struct BookingVoucherData
    {
        public BookingVoucherData(int bookingId, in AccommodationInfo accommodation, in DateTime checkInDate, in DateTime checkOutDate, in DeadlineDetails deadlineDetails, string mainPassengerName, string referenceCode, List<BookingRoomDetails> roomDetails)
        {
            Accommodation = accommodation;
            BookingId = bookingId;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            DeadlineDetails = deadlineDetails;
            MainPassengerName = mainPassengerName;
            ReferenceCode = referenceCode;
            RoomDetails = roomDetails;
        }


        public int BookingId { get; }
        public AccommodationInfo Accommodation {get;}
        public DateTime CheckInDate { get; }
        public DateTime CheckOutDate { get; }
        public DeadlineDetails DeadlineDetails { get; }
        public string MainPassengerName { get; }
        public string ReferenceCode { get; }
        public List<BookingRoomDetails> RoomDetails { get; }


        public struct AccommodationInfo
        {
            public AccommodationInfo(string name, in SlimLocationInfo locationInfo, in ContactInfo contactInfo)
            {
                ContactInfo = contactInfo;
                Location = locationInfo;
                Name = name;
            }


            public ContactInfo ContactInfo { get; }
            public SlimLocationInfo Location { get; }
            public string Name { get; }
        }
    }
}