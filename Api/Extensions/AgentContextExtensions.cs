using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class AgentContextExtensions
    {
        public static bool IsUsingAgency(this AgentContext agentContext, int agencyId) =>
            agentContext.AgencyId == agencyId;


        public static bool IsInCounterparty(this AgentContext agentContext, int counterpartyId) =>
            agentContext.CounterpartyId == counterpartyId;
    }
}
