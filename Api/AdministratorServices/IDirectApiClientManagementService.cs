using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IDirectApiClientManagementService
    {
        Task<List<DirectApiClientSlim>> GetAllClients();

        Task<Result<DirectApiClientSlim>> GetById(string clientId);

        Task<Result> Create(CreateDirectApiClientRequest request);

        Task<Result> Delete(string clientId);

        Task<Result> Activate(string clientId);

        Task<Result> Deactivate(string clientId);

        Task<Result> BindToAgent(string clientId, int agentId);

        Task<Result> UnbindFromAgent(string clientId, int agentId);
    }
}