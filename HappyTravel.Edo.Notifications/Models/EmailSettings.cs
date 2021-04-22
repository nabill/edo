using System.Collections.Generic;

namespace HappyTravel.Edo.Notifications.Models
{
    public class EmailSettings : ISendingSettings
    {
        public List<string> Emails { get; set; }
        public string TemplateId { get; set; }
    }
}