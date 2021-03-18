using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums.Notifications;

namespace HappyTravel.Edo.Data.Notifications
{
    public class NotificationOption : IEntity
    {
        public int Id { get; set; }
        public int AgentId { get; set; }
        public int AgencyId { get; set; }
        public NotificationType Type { get; set; }
        public ProtocolTypes EnabledProtocols { get; set; }
        public bool IsMandatory { get; set; }
    }
}