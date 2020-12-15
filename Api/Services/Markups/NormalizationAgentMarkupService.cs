using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Services.Markups.Templates;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class NormalizationAgentMarkupService : INormalizationAgentMarkupService
    {
        public NormalizationAgentMarkupService(EdoContext context, IMarkupPolicyTemplateService markupPolicyTemplateService)
        {
            _context = context;
            _markupPolicyTemplateService = markupPolicyTemplateService;
        }


        public async Task<Result> UpdateMarkup(int agentId)
        {
            return await GetAgent()
                .Tap(SetAgentMarkup);


            async Task<Result<Agent>> GetAgent()
            {
                var agent = await _context.Agents
                    .SingleOrDefaultAsync(a => a.Id == agentId);

                return agent ?? Result.Failure<Agent>($"Agent with id {agentId} not found");
            }


            async Task SetAgentMarkup(Agent agent)
            {
                var normalizedMarkup = await GetAgentMarkup(agent.Id);
                agent.NormalizedMarkup = normalizedMarkup;
                _context.Agents.Update(agent);
                await _context.SaveChangesAsync();
            }
        }


        private async Task<string> GetAgentMarkup(int agentId)
        {
            var policies = await _context.MarkupPolicies
                .Where(p => p.AgentId == agentId && p.ScopeType == MarkupPolicyScopeType.Agent)
                .ToListAsync();

            return !policies.Any()
                ? string.Empty
                : _markupPolicyTemplateService.GetMarkupsFormula(policies);
        }


        private readonly EdoContext _context;
        private readonly IMarkupPolicyTemplateService _markupPolicyTemplateService;
    }
}