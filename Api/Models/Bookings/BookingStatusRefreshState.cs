using System;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    public record BookingStatusRefreshState
    {
        public int BookingId { get; init; }
        public int RefreshStatusCount { get; init; }
        public DateTimeOffset LastRefreshDate { get; init; }
        public DateTimeOffset? DeadlineDate { get; init; }
    }
}