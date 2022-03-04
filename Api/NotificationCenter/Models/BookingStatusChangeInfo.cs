using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;
using System;

namespace HappyTravel.Edo.Api.NotificationCenter.Models
{
    public readonly struct BookingStatusChangeInfo
    {
        public int BookingId { get; init; }
        public string ReferenceCode { get; init; }
        public string Status { get; init; }
        public DateTimeOffset ChangeTime { get; init; }
        public string AccommodationName { get; init; }
        public ImageInfo AccommodationPhoto { get; init; }
        public DateTimeOffset CheckInDate { get; init; }
        public DateTimeOffset CheckOutDate { get; init; }
    }
}
