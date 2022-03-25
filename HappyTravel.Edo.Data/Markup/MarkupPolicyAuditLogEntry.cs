using System;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Data.Markup
{
    public class MarkupPolicyAuditLogEntry
    {
        public int Id { get; set; }
        public MarkupPolicyEventType Type { get; set; }
        public DateTimeOffset Created { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApiCallerTypes ApiCallerType { get; set; }
        public string? EventData { get; set; }
    }
}
