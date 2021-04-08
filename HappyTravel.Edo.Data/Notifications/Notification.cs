using System;
using System.Collections.Generic;
using HappyTravel.Edo.Notifications.Enums;
using HappyTravel.Edo.Notifications.Models;

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