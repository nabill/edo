using System;
using System.Collections.Generic;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.Accommodations.Internals;

namespace HappyTravel.Edo.Api.Models.Mailing
{
    public readonly struct BookingVoucherData
    {
        public BookingVoucherData(int bookingId, in AccommodationInfo accommodation,
            in DateTime checkInDate, in DateTime checkOutDate, in DateTime? deadlineDate, 
            string mainPassengerName, string referenceCode, List<BookedRoom> roomDetails, string accommodationName)
        {
            Accommodation = accommodation;
            BookingId = bookingId;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            DeadlineDate = deadlineDate;
            MainPassengerName = mainPassengerName;
            ReferenceCode = referenceCode;
            RoomDetails = roomDetails;
            AccommodationName = accommodationName;
        }


        public int BookingId { get; }
        public AccommodationInfo Accommodation {get;}
        public DateTime CheckInDate { get; }
        public DateTime CheckOutDate { get; }
        public DateTime? DeadlineDate { get; }
        public string MainPassengerName { get; }
        public string ReferenceCode { get; }
        public List<BookedRoom> RoomDetails { get; }
        public string AccommodationName { get; }


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