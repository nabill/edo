using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.UnitTests.Utility
{
    public static class AgentInfoFactory
    {
        public static AgentContext GetByAgentId(int agentId)
        {
            return new AgentContext(agentId, string.Empty, string.Empty, string.Empty, string.Empty,  string.Empty, 0, string.Empty, default, true, InAgencyPermissions.All);
        }


        public static AgentContext CreateWithCounterpartyAndAgency(int agentId, int counterpartyId, int agencyId)
        {
            return new AgentContext(agentId, string.Empty, string.Empty, string.Empty, string.Empty,  string.Empty, counterpartyId, string.Empty, agencyId, true, InAgencyPermissions.All);
        }
    }
}