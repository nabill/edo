using System;

namespace HappyTravel.Edo.Api.Models.Reports
{
    public readonly struct PendingSupplierReference
    {
        public DateTime Date { get; init; }
        public string ReferenceCode { get; init; }
    }
}