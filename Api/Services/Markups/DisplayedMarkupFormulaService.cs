using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Services.Markups.Templates;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Markup;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class DisplayedMarkupFormulaService : IDisplayedMarkupFormulaService
    {
        public DisplayedMarkupFormulaService(EdoContext context, IMarkupPolicyTemplateService markupPolicyTemplateService)
        {
            _context = context;
            _markupPolicyTemplateService = markupPolicyTemplateService;
        }


        public async Task<Result> Update(int agentId, int agencyId)
        {
            var counterpartyId = await (from relation in _context.AgentAgencyRelations
                join agency in _context.Agencies on relation.AgencyId equals agency.Id
                where relation.AgencyId == agencyId && relation.AgentId == agentId
                select agency.CounterpartyId).SingleOrDefaultAsync();

            if (counterpartyId == default)
                return Result.Failure<Agent>($"Agent with id {agentId} not found in agency with id {agencyId}");

            var formula = await GetAgentMarkupFormula(agentId, agencyId);
            var displayedMarkupFormula = await _context.DisplayMarkupFormulas
                .SingleOrDefaultAsync(f => f.AgencyId == agencyId && f.AgentId == agentId);

            if (displayedMarkupFormula is null)
            {
                _context.DisplayMarkupFormulas.Add(new DisplayMarkupFormula
                {
                    CounterpartyId = counterpartyId,
                    AgencyId = agencyId,
                    AgentId = agentId,
                    DisplayFormula = formula
                });
            }
            else
            {
                displayedMarkupFormula.DisplayFormula = formula;
                _context.DisplayMarkupFormulas.Update(displayedMarkupFormula);
            }
            
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        
        
        public async Task<Result> Update(int agencyId)
        {
            var isAgencyExists = await _context.Agencies
                .AnyAsync(a => a.Id == agencyId);
            
            if(!isAgencyExists)
                return Result.Failure($"Agency with id '{agencyId}' not found");
            
            var displayedMarkupFormula = await GetAgencyMarkupFormula(agencyId);
            
            // TODO: implement saving displayed markup formula

            return Result.Success();
        }


        private async Task<string> GetAgentMarkupFormula(int agentId, int agencyId)
        {
            var policies = await _context.MarkupPolicies
                .Where(p => p.AgentId == agentId && p.AgencyId == agencyId && p.ScopeType == MarkupPolicyScopeType.Agent)
                .OrderBy(p => p.Order)
                .ToListAsync();

            return policies.Any()
                ? _markupPolicyTemplateService.GetMarkupsFormula(policies)
                : string.Empty;
        }
        
        
        private async Task<string> GetAgencyMarkupFormula(int agencyId)
        {
            var policies = await _context.MarkupPolicies
                .Where(p => p.AgencyId == agencyId && p.ScopeType == MarkupPolicyScopeType.Agency)
                .OrderBy(p => p.Order)
                .ToListAsync();

            return policies.Any()
                ? _markupPolicyTemplateService.GetMarkupsFormula(policies)
                : string.Empty;
        }


        private readonly EdoContext _context;
        private readonly IMarkupPolicyTemplateService _markupPolicyTemplateService;
    }
}