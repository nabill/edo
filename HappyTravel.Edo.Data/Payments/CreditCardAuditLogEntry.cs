using System;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Data.Payments
{
    public class CreditCardAuditLogEntry
    {
        public int Id { get; set; }
        public CreditCardEventType Type { get; set; }
        public DateTimeOffset Created { get; set; }
        public int AgentId { get; set; }
        public string UserId { get; set; }
        public ApiCallerTypes ApiCallerType { get; set; }
        public string MaskedNumber { get; set; }
        public decimal Amount { get; set; }
        public string EventData { get; set; }
        public string ReferenceCode { get; set; }
        public Currencies Currency { get; set; }
    }
}
