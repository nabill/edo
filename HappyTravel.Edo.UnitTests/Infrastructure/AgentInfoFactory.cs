using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.UnitTests.Infrastructure
{
    public static class AgentInfoFactory
    {
        public static AgentInfo GetByAgentId(int agentId)
        {
            return new AgentInfo(agentId, string.Empty, string.Empty, string.Empty, string.Empty,  string.Empty, 0, string.Empty, default, true, InCounterpartyPermissions.All);
        }


        public static AgentInfo CreateByWithCounterpartyAndAgency(int agentId, int counterpartyId, int agencyId)
        {
            return new AgentInfo(agentId, string.Empty, string.Empty, string.Empty, string.Empty,  string.Empty, counterpartyId, string.Empty, agencyId, true, InCounterpartyPermissions.All);
        }
    }
}