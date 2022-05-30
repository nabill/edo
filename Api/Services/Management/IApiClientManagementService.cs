using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.Services.Management
{
    public interface IApiClientManagementService
    {
        Task<Result> Set(int agencyId, int agentId, ApiClientData clientData);
        Task<Result> Delete(int agencyId, int agentId);
        Task<Result<ApiClientData>> Generate(AgentContext agentContext);
    }
}