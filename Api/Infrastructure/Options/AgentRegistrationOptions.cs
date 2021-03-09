using System.Collections.Generic;

namespace HappyTravel.Edo.Api.Infrastructure.Options
{
    public class AgentRegistrationNotificationOptions
    {
        public string MasterAgentMailTemplateId { get; set; }
        public string RegularAgentMailTemplateId { get; set; }
        public string ChildAgencyMailTemplateId { get; set; }
        public List<string> AdministratorsEmails { get; set; }
    }
}