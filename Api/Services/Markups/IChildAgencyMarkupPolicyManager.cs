using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Markups.Agency;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public interface IChildAgencyMarkupPolicyManager
    {
        Task<Result> Set(int agencyId, SetAgencyMarkupRequest request, AgentContext agent);

        Task<Result> Remove(int agencyId, AgentContext agent);

        Task<Result<AgencyMarkupInfo?>> Get(int agencyId, AgentContext agent);
    }
}