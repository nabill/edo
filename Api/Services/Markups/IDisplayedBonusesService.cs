using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public interface IDisplayedBonusesService
    {
        IQueryable<Bonus> GetList(AgentContext agentContext);

        Task<BonusSummary> GetSum(BonusSummaryFilter filter, AgentContext agentContext);
    }
}