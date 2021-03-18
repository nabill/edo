using System;
using HappyTravel.Edo.Common.Enums.Notifications;

namespace HappyTravel.Edo.Data.Notifications
{
    public class Notification : IEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; }
        public ProtocolTypes[] Protocols { get; set; }
        public string EmailSettings { get; set; }
        public DateTime Created { get; set; }
        public bool IsRead { get; set; }
    }
}