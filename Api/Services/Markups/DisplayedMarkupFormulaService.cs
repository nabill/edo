using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Markups.Templates;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
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


        public async Task<Result> UpdateAgentFormula(int agentId, int agencyId)
        {
            var formula = await GetAgentMarkupFormula(agentId, agencyId);
            var displayedMarkupFormula = await _context.DisplayMarkupFormulas
                .SingleOrDefaultAsync(f => f.AgencyId == agencyId && f.AgentId == agentId);

            if (displayedMarkupFormula is null)
            {
                _context.DisplayMarkupFormulas.Add(new DisplayMarkupFormula
                {
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
        
        
        public async Task<Result> UpdateAgencyFormula(int agencyId)
        {
            var formula = await GetAgencyMarkupFormula(agencyId);
            var displayedMarkupFormula = await _context.DisplayMarkupFormulas
                .SingleOrDefaultAsync(f => f.AgencyId == agencyId && f.AgentId == null);
            
            if (displayedMarkupFormula is null)
            {
                _context.DisplayMarkupFormulas.Add(new DisplayMarkupFormula
                {
                    AgencyId = agencyId,
                    AgentId = null,
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


        public async Task<Result> UpdateGlobalFormula()
        {
            var displayedMarkupFormula = await _context.DisplayMarkupFormulas
                .SingleOrDefaultAsync(f => f.AgencyId == null && f.AgentId == null);
            
            var formula = await GetGlobalMarkupFormula();
            if (displayedMarkupFormula is null)
            {
                _context.DisplayMarkupFormulas.Add(new DisplayMarkupFormula
                {
                    AgencyId = null,
                    AgentId = null,
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


        private async Task<string> GetAgentMarkupFormula(int agentId, int agencyId)
        {
            var agentScopeId = AgentInAgencyId.Create(agentId, agencyId).ToString();
            var policies = await _context.MarkupPolicies
                .Where(p => p.SubjectScopeId == agentScopeId && p.SubjectScopeType == SubjectMarkupScopeTypes.Agent)
                .OrderBy(p => p.FunctionType)
                .ToListAsync();

            return policies.Any()
                ? _markupPolicyTemplateService.GetMarkupsFormula(policies)
                : string.Empty;
        }
        
        
        private async Task<string> GetAgencyMarkupFormula(int agencyId)
        {
            var policies = await _context.MarkupPolicies
                .Where(p => p.SubjectScopeId == agencyId.ToString() && p.SubjectScopeType == SubjectMarkupScopeTypes.Agency)
                .OrderBy(p => p.FunctionType)
                .ToListAsync();

            return policies.Any()
                ? _markupPolicyTemplateService.GetMarkupsFormula(policies)
                : string.Empty;
        }
        
        
        private async Task<string> GetGlobalMarkupFormula()
        {
            var policies = await _context.MarkupPolicies
                .Where(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Global)
                .OrderBy(p => p.FunctionType)
                .ToListAsync();

            return policies.Any()
                ? _markupPolicyTemplateService.GetMarkupsFormula(policies)
                : string.Empty;
        }


        private readonly EdoContext _context;
        private readonly IMarkupPolicyTemplateService _markupPolicyTemplateService;
    }
}