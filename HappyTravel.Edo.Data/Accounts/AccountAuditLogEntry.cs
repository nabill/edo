using System;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Data.Accounts
{
    public class AccountAuditLogEntry
    {
        public int Id { get; set; }
        public AccountEventType Type { get; set; }
        public DateTime Created { get; set; }
        public int? AdministratorId { get; set; }
        public string EventData { get; set; }
    }
}
