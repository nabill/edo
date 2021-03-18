using System;

namespace HappyTravel.Edo.Data.Notifications
{
    public class Notification : IEntity
    {
        public int Id { get; set; }
        public int AgentId { get; set; }
        public string Message { get; set; }
        public string Protocols { get; set; }
        public string EmailSettings { get; set; }
        public DateTime Created { get; set; }
        public bool IsRead { get; set; }
    }
}