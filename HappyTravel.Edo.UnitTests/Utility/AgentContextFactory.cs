using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.UnitTests.Utility
{
    public static class AgentContextFactory
    {
        public static AgentContext CreateByAgentId(int agentId)
        {
            return new(agentId, string.Empty, string.Empty, string.Empty, string.Empty,  string.Empty, 0, string.Empty, true, InAgencyPermissions.All, "", "", new());
        }


        public static AgentContext CreateWithCounterpartyAndAgency(int agentId, int agencyId)
        {
            return new(agentId, string.Empty, string.Empty, string.Empty, string.Empty,  string.Empty, agencyId, string.Empty, true, InAgencyPermissions.All, "", "", new());
        }
    }
}