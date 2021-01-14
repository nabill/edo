using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.UnitTests.Utility
{
    public static class AgentContextFactory
    {
        public static AgentContext CreateByAgentId(int agentId)
        {
            return new(agentId, string.Empty, string.Empty, string.Empty, string.Empty,  string.Empty, 0, string.Empty, default, true, InAgencyPermissions.All);
        }


        public static AgentContext CreateWithCounterpartyAndAgency(int agentId, int counterpartyId, int agencyId)
        {
            return new(agentId, string.Empty, string.Empty, string.Empty, string.Empty,  string.Empty, counterpartyId, string.Empty, agencyId, true, InAgencyPermissions.All);
        }
    }
}