using System;
using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums.Notifications;
using HappyTravel.Edo.Common.Models.Notifications;

namespace HappyTravel.Edo.Data.Notifications
{
    public class Notification : IEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; }
        public Dictionary<ProtocolTypes, ISendingSettings> SendingSettings { get; set; }
        public DateTime Created { get; set; }
        public bool IsRead { get; set; }
    }
}