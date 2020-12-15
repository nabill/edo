using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public interface INormalizationAgentMarkupService
    {
        public Task<Result> UpdateMarkup(int agentId);
    }
}