using System;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Data.Payments
{
    public class MarkupPaymentLog
    {
        public int Id { get; set; }
        public int AgencyAccountId { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal Amount { get; set; }
        public Currencies Currency { get; set; }
        public int BookingMarkupId { get; set; }
        public string ReferenceCode { get; set; }
    }
}