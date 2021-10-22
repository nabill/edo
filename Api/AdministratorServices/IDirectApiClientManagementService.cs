using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IDirectApiClientManagementService
    {
        Task<Result<List<DirectApiClientSlim>, ProblemDetails>> GetAllClients();

        Task<Result<DirectApiClientSlim, ProblemDetails>> GetById(string clientId);

        Task<Result<Unit, ProblemDetails>> Create(CreateDirectApiClientRequest request);

        Task<Result<Unit, ProblemDetails>> Delete(string clientId);

        Task<Result<Unit, ProblemDetails>> Activate(string clientId);

        Task<Result<Unit, ProblemDetails>> Deactivate(string clientId);

        Task<Result> BindToAgent(string clientId, int agentId);

        Task<Result> UnbindFromAgent(string clientId, int agentId);
    }
}