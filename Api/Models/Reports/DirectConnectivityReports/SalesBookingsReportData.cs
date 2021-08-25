using System;
using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Money.Enums;
using HappyTravel.SuppliersCatalog;

namespace HappyTravel.Edo.Api.Models.Reports.DirectConnectivityReports
{
    public readonly struct SalesBookingsReportData
    {
        public string ReferenceCode { get; init; }
        public string InvoiceNumber { get; init; }
        public BookingStatuses BookingStatus { get; init; }
        public string AccommodationName { get; init; }
        public string ConfirmationNumber { get; init; }
        public string AgencyName { get; init; }
        public PaymentTypes PaymentMethod { get; init; }
        public string GuestName { get; init; }
        public DateTime Created { get; init; }
        public DateTime ArrivalDate { get; init; }
        public DateTime DepartureDate { get; init; }
        public decimal SupplierPrice { get; init; }
        public Currencies SupplierCurrency { get; init; }
        public decimal SupplierConvertedPrice { get; init; }
        public Currencies SupplierConvertedCurrency { get; init; }
        public List<BookedRoom> Rooms { get; init; }
        public Suppliers Supplier { get; init; }
        public BookingPaymentStatuses PaymentStatus { get; init; }
        public List<CancellationPolicy> CancellationPolicies { get; init; }
        public DateTime? CancellationDate { get; init; }
        public bool IsDirectContract { get; init; }
        public DateTime CheckInDate { get; init; }
        public DateTime CheckOutDate { get; init; }
        public decimal AgentPrice { get; init; }
        public Currencies AgentCurrency { get; init; }
        public DateTime? AgentDeadline { get; init; }
        public Deadline? SupplierDeadline { get; init; }
    }
}