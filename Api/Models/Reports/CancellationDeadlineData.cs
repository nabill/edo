using System;

namespace HappyTravel.Edo.Api.Models.Reports
{
    public readonly struct CancellationDeadlineData
    {
        public string ReferenceCode { get; init; }
        public string AccommodationName { get; init; }
        public string BookingStatus { get; init; }
        public DateTime Created { get; init; }
        public DateTime CheckInDate  { get; init; }
        public DateTime CheckOutDate { get; init; }
        public DateTime DeadlineDate { get; init; }
    }
}