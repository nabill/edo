using System;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Data.Payments
{
    public class CreditCardAuditLogEntry
    {
        public int Id { get; set; }
        public CreditCardEventType Type { get; set; }
        public DateTime Created { get; set; }
        public int UserId { get; set; }
        public UserTypes UserType { get; set; }
        public string MaskedNumber { get; set; }
        public decimal Amount { get; set; }
        public string EventData { get; set; }
        public string ReferenceCode { get; set; }
    }
}
