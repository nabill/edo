using HappyTravel.Edo.Common.Enums;
using System;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Data.Payments
{
    public class Payment
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public decimal RefundedAmount { get; set; }
        public string ReferenceCode { get; set; }
        public int? AccountId { get; set; }
        public Currencies Currency { get; set; }
        public string AccountNumber { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Modified { get; set; }
        public PaymentStatuses Status { get; set; }
        public PaymentTypes PaymentMethod { get; set; }
        public PaymentProcessors PaymentProcessor { get; set; }
        public string Data { get; set; }
        public string CaptureId { get; set; }
    }
}
