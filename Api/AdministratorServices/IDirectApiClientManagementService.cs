using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IDirectApiClientManagementService
    {
        Task<Result> AddApiClient(CreateDirectApiClientRequest request);

        Task<Result> RemoveApiClient(RemoveDirectApiClientRequest request);

        Task<Result> ChangePassword(string clientId, string password);
    }
}