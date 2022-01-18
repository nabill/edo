using System;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Money.Enums;
using HappyTravel.SuppliersCatalog;

namespace HappyTravel.Edo.Api.AdministratorServices.Models
{
    public class BookingSlim
    {
        public int Id { get; set; }
        public string ReferenceCode { get; set; }
        public BookingPaymentStatuses PaymentStatus { get; set; }
        public int AgentId { get; set; }
        public int AgencyId { get; set; }
        public string AgentName { get; set; }
        public string AgencyName { get; set; }
        public string HtId { get; set; }
        public string AccommodationName { get; set; }
        public DateTime Created { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public DateTime? DeadlineDate { get; set; }
        public decimal TotalPrice { get; set; }
        public Currencies Currency { get; set; }
        public BookingStatuses Status { get; set; }
        public Suppliers Supplier { get; set; }
    }
}