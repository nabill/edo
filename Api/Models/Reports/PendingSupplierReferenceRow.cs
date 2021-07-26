using System;
using System.Collections.Generic;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.Api.Models.Reports
{
    public struct PendingSupplierReferenceRow
    {
        public DateTime Created { get; init; }
        public string ReferenceCode { get; init; }
        public string AccommodationName { get; init; }
        public DateTime CheckInDate  { get; init; }
        public DateTime CheckOutDate { get; init; }
        public int NumberOfPassengers { get; init; }
    }
}