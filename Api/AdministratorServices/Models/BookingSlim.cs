using System;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.AdministratorServices.Models
{
    public class BookingSlim
    {
        public int Id { get; set; }
        public string ReferenceCode { get; set; }
        public BookingPaymentStatuses PaymentStatus { get; set; }
        public int AgentId { get; set; }
        public int AgencyId { get; set; }
        public string AccommodationName { get; set; }
        public DateTime Created { get; set; }
        public decimal TotalPrice { get; set; }
        public Currencies Currency { get; set; }
    }
}