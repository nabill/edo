using System;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Data.Management
{
    public class ManagementAuditLogEntry
    {
        public ManagementEventType Type { get; set; }
        public DateTime Created { get; set; }
        public int AdministratorId { get; set; }
        public string EventData { get; set; }
    }
}