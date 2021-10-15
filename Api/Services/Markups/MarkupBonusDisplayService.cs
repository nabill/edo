using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class MarkupBonusDisplayService : IMarkupBonusDisplayService
    {
        public MarkupBonusDisplayService(EdoContext context)
        {
            _context = context;
        }
        
        
        public IQueryable<Bonus> GetBonuses(AgentContext agentContext)
        {
            // TODO: https://github.com/happy-travel/agent-app-project/issues/731
            return new List<Bonus>().AsQueryable();
            // return from appliedMarkup in _context.AppliedBookingMarkups
            //    join markupPolicy in _context.MarkupPolicies on appliedMarkup.PolicyId equals markupPolicy.Id
            //     join agentAgency in _context.AgentAgencyRelations on markupPolicy.AgentId equals agentAgency.AgentId
            //     join booking in _context.Bookings on appliedMarkup.ReferenceCode equals booking.ReferenceCode
            //     where agentAgency.AgencyId == agentContext.AgencyId
            //     select new Bonus
            //     {
            //         ReferenceCode = appliedMarkup.ReferenceCode,
            //         Created = booking.Created,
            //         Paid = appliedMarkup.Paid,
            //         Amount = new MoneyAmount(appliedMarkup.Amount, markupPolicy.Currency)
            //     };
        }


        public async Task<BonusSummary> GetBonusesSummary(BonusSummaryFilter filter, AgentContext agentContext)
        {
            // TODO: https://github.com/happy-travel/agent-app-project/issues/731
            return new BonusSummary();

            // var query = from appliedMarkup in _context.AppliedBookingMarkups
            //     join markupPolicy in _context.MarkupPolicies on appliedMarkup.PolicyId equals markupPolicy.Id
            //     join agentAgency in _context.AgentAgencyRelations on markupPolicy.AgentId equals agentAgency.AgentId
            //     where agentAgency.AgencyId == agentContext.AgencyId
            //     select new {appliedMarkup.Paid, appliedMarkup.Amount, markupPolicy.Currency};
            // 

            // if (filter.From is not null)
            //     query = query.Where(x => x.Paid >= filter.From.Value);

            // if (filter.To is not null)
            //     query = query.Where(x => x.Paid <= filter.To.Value);

            // var summary = await query
            //     .GroupBy(x => x.Currency)
            //     .Select(x => new MoneyAmount (x.Sum(g => g.Amount), x.Key))
            //     .ToListAsync();

            // return new BonusSummary { Summary = summary };
        }


        private readonly EdoContext _context;
    }
}