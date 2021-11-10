using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public interface IDisplayedMarkupFormulaService
    {
        public Task<Result> UpdateAgentFormula(int agentId, int agencyId);
        public Task<Result> UpdateAgencyFormula(int agencyId);
        public Task<Result> UpdateGlobalFormula();
    }
}