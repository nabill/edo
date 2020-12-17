using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore;
using Prometheus;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class MarkupBonusDisplayService : IMarkupBonusDisplayService
    {
        public MarkupBonusDisplayService(EdoContext context)
        {
            _context = context;
        }
        
        
        public IQueryable<Bonus> GetBonusesList(AgentContext agentContext)
        {
            return from appliedMarkup in _context.AppliedBookingMarkups
                join markupPolicy in _context.MarkupPolicies on appliedMarkup.PolicyId equals markupPolicy.Id
                join agentAgency in _context.AgentAgencyRelations on markupPolicy.AgentId equals agentAgency.AgentId
                where agentAgency.AgencyId == agentContext.AgencyId
                select new Bonus
                {
                    ReferenceCode = appliedMarkup.ReferenceCode,
                    Paid = appliedMarkup.Paid,
                    Amount = new MoneyAmount(appliedMarkup.Amount, markupPolicy.Currency)
                };
        }


        public async Task<BonusSummary> GetBonusesSummary(BonusSummaryFilter filter, AgentContext agentContext)
        {
            var query = from appliedMarkup in _context.AppliedBookingMarkups
                join markupPolicy in _context.MarkupPolicies on appliedMarkup.PolicyId equals markupPolicy.Id
                join agentAgency in _context.AgentAgencyRelations on markupPolicy.AgentId equals agentAgency.AgentId
                where agentAgency.AgencyId == agentContext.AgencyId
                select new {appliedMarkup.Paid, appliedMarkup.Amount, markupPolicy.Currency};

            if (filter.From is not null)
                query = query.Where(x => x.Paid >= filter.From.Value);

            if (filter.To is not null)
                query = query.Where(x => x.Paid <= filter.To.Value);

            var summary = await query
                .GroupBy(x => x.Currency)
                .Select(x => new MoneyAmount (x.Sum(g => g.Amount), x.Key))
                .ToListAsync();
            
            return new BonusSummary { Summary = summary };
        }


        private readonly EdoContext _context;
    }
}