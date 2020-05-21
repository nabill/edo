using System;
using System.Collections.Generic;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.Accommodations.Internals;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    public readonly struct BookingVoucherData
    {
        public BookingVoucherData(string agentName, int bookingId, in AccommodationInfo accommodation, int nightCount,
            in DateTime checkInDate, in DateTime checkOutDate, DateTime? deadlineDate, 
            string mainPassengerName, string referenceCode, List<BookedRoom> roomDetails)
        {
            AgentName = agentName;
            Accommodation = accommodation;
            NightCount = nightCount;
            BookingId = bookingId;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            DeadlineDate = deadlineDate;
            MainPassengerName = mainPassengerName;
            ReferenceCode = referenceCode;
            RoomDetails = roomDetails;
        }


        public int BookingId { get; }
        public string AgentName { get; }
        public AccommodationInfo Accommodation {get;}
        public int NightCount { get; }
        public DateTime CheckInDate { get; }
        public DateTime CheckOutDate { get; }
        public DateTime? DeadlineDate { get; }
        public string MainPassengerName { get; }
        public string ReferenceCode { get; }
        public List<BookedRoom> RoomDetails { get; }


        public readonly struct AccommodationInfo
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