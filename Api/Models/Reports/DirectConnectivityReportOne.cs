using System;
using System.Collections.Generic;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.Api.Models.Reports
{
    public record DirectConnectivityReportOne
    {
        public string ReferenceCode { get; init; }
        public string InvoiceNumber { get; init; }
        public string HotelName { get; init; }
        public string HotelConfirmationNumber { get; init; }
        public List<BookedRoom> Rooms { get; init; }
        public string GuestName { get; init; }
        public DateTime ArrivalDate { get; init; }
        public DateTime DepartureDate { get; init; }
        public decimal TotalAmount { get; init; }
    }
}