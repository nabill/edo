using System;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Data.Payments
{
    public class OfflinePaymentAuditLogEntry
    {
        public int Id { get; set; }
        public DateTimeOffset Created { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApiCallerTypes ApiCallerType { get; set; }
        public string? ReferenceCode { get; set; }
    }
}
