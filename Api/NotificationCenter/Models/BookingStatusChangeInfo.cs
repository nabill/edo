using HappyTravel.Edo.Common.Enums;
using System;

namespace HappyTravel.Edo.Api.NotificationCenter.Models
{
    public readonly struct BookingStatusChangeInfo
    {
        public int BookingId { get; init; }
        public string ReferenceCode { get; init; }
        public BookingStatuses Status { get; init; }
        public DateTime ChangeTime { get; init; }
        public string AccommodationName { get; init; }
        public string AccommodationPhoto { get; init; }
        public DateTime CheckInDate { get; init; }
        public DateTime CheckOutDate { get; init; }
    }
}
