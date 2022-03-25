using System;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Data.Payments
{
    public class AccountBalanceAuditLogEntry
    {
        public int Id { get; set; }
        public AccountEventType Type { get; set; }
        public DateTimeOffset Created { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApiCallerTypes ApiCallerType { get; set; }
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
        public string EventData { get; set; } = string.Empty;
        public string? ReferenceCode { get; set; }
    }
}
