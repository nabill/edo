using System;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Data.PaymentLinks
{
    public class PaymentLink
    {
        public string Code { get; set; } = string.Empty;
        public string? Email { get; set; }
        public Currencies Currency { get; set; }
        public ServiceTypes ServiceType { get; set; }
        public decimal Amount { get; set; }
        public string? Comment { get; set; }
        public string ReferenceCode { get; set; } = string.Empty;
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? LastPaymentDate { get; set; }
        public string? LastPaymentResponse { get; set; }
        public string? ExternalId { get; set; }
        public PaymentProcessors? PaymentProcessor { get; set; }
        public string? InvoiceNumber { get; set; }
        public int? AdministratorId { get; set; }
    }
}