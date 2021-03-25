using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public interface IAgencyService
    {
        Task<Result<AgencyInfo>> GetAgency(int agencyId, AgentContext agent, string languageCode = LocalizationHelper.DefaultLanguageCode);

        Task<List<AgencyInfo>> GetChildAgencies(AgentContext agent);
    }
}
