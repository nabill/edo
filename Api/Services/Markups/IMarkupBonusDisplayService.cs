using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public interface IMarkupBonusDisplayService
    {
        IQueryable<Bonus> GetBonuses(AgentContext agentContext);

        Task<BonusSummary> GetBonusesSummary(BonusSummaryFilter filter, AgentContext agentContext);
    }
}