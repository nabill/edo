using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.Services
{
    public interface IAgencyManagementService
    {
        Task<Result> DeactivateAgency(int agencyId, AgentContext agent);

        Task<Result> ActivateAgency(int agencyId, AgentContext agent);
    }
}