using System;
using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.Api.Models.Reports
{
    public readonly struct HotelWiseData
    {
        public DateTime Created { get; init; }
        public string AccommodationName { get; init; }
        public string ReferenceCode { get; init; }
        public BookingStatuses BookingStatus { get; init; }
        public DateTime CheckInDate  { get; init; }
        public DateTime CheckOutDate { get; init; }
        public List<BookedRoom> Rooms { get; init; }
    }
}