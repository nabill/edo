using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Markups.Abstractions;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public static class AgentContextMarkupExtensions
    {
        public static MarkupSubjectInfo ToMarkupSubjectInfo(this AgentContext agentContext)
            => new()
            {
                AgentId = agentContext.AgentId,
                AgencyId = agentContext.AgencyId,
                CountryHtId = agentContext.CountryHtId,
                LocalityHtId = agentContext.LocalityHtId,
                AgencyAncestors = agentContext.AgencyAncestors
            };
    }
}