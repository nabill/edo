using System;

namespace HappyTravel.Edo.Api.Models.Reports
{
    public readonly struct PendingSupplierReference
    {
        public DateTime Created { get; init; }
        public string ReferenceCode { get; init; }
        public string Hotel { get; init; }
        public DateTime CheckInDate  { get; init; }
        public DateTime CheckOutDate { get; init; }
        public int Passengers { get; init; }
    }
}