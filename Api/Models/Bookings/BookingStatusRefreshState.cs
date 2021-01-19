using System;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    public record BookingStatusRefreshState
    {
        public int Id { get; init; }
        public int RefreshStatusCount { get; init; }
        public DateTime LastRefreshingDate { get; init; }
    }
}