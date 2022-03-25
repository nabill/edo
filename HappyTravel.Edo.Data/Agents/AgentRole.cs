using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Data.Agents
{
    public class AgentRole
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public InAgencyPermissions Permissions { get; set; }
        public bool IsPreservedInAgency { get; set; }
    }
}