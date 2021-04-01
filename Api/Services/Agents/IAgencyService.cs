using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public interface IAgencyService
    {
        Task<Result<SlimAgencyInfo>> Get(AgentContext agent, string languageCode = LocalizationHelper.DefaultLanguageCode);
    }
}