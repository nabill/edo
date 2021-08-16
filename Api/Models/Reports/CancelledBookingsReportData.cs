using System;

namespace HappyTravel.Edo.Api.Models.Reports
{
    public readonly struct CancelledBookingsReportData
    {
        public string ReferenceCode { get; init; }
        public string Supplier { get; init; }
        public string AccommodationName { get; init; }
        public string AgentName { get; init; }
        public string AgencyName { get; init; }
        public DateTime Created { get; init; }
        public DateTime CheckInDate { get; init; }
        public DateTime CheckOutDate { get; init; }
        public DateTime DeadlineDate { get; init; }
    }
}