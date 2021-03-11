using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.Services
{
    public interface IAgencyManagementService
    {
        Task<Result> DeactivateChildAgency(int agencyId, AgentContext agent);

        Task<Result> ActivateChildAgency(int agencyId, AgentContext agent);
    }
}