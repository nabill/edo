using System;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Data.Payments
{
    public class Refund
    {
        public int Id { get; set; }
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public Currencies Currency { get; set; }
        public DateTime PlannedDate { get; set; }
    }
}