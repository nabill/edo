using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;
using Prometheus;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class DisplayedBonusesService : IDisplayedBonusesService
    {
        public DisplayedBonusesService(EdoContext context)
        {
            _context = context;
        }
        
        
        public IQueryable<Bonus> GetList(AgentContext agentContext)
        {
            return from appliedMarkup in _context.AppliedBookingMarkups
                join markupPolicy in _context.MarkupPolicies on appliedMarkup.PolicyId equals markupPolicy.Id
                join agentAgency in _context.AgentAgencyRelations on markupPolicy.AgentId equals agentAgency.AgentId
                where agentAgency.AgencyId == agentContext.AgencyId
                select new Bonus
                {
                    ReferenceCode = appliedMarkup.ReferenceCode,
                    Paid = appliedMarkup.Paid,
                    Amount = appliedMarkup.Amount
                };
        }


        public async Task<BonusSummary> GetSum(BonusSummaryFilter filter, AgentContext agentContext)
        {
            var query = from appliedMarkup in _context.AppliedBookingMarkups
                join markupPolicy in _context.MarkupPolicies on appliedMarkup.PolicyId equals markupPolicy.Id
                join agentAgency in _context.AgentAgencyRelations on markupPolicy.AgentId equals agentAgency.AgentId
                where agentAgency.AgencyId == agentContext.AgencyId
                select new {appliedMarkup.Paid, appliedMarkup.Amount};

            if (filter.From is not null)
                query = query.Where(x => x.Paid >= filter.From.Value);

            if (filter.Till is not null)
                query = query.Where(x => x.Paid <= filter.Till.Value);

            var summary = await query.SumAsync(x => x.Amount);
            return new BonusSummary { Summary = summary};
        }


        private readonly EdoContext _context;
    }
}