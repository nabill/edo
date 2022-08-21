using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.Services.Management
{
    public interface IDirectApiClientManagementService
    {
        Task<Result<ApiClientData>> Generate(AgentContext agent);
        Task<Result> RemoveApiClient(RemoveDirectApiClientRequest request);
    }
}