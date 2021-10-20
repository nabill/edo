using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Models.Agents;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class DirectApiClientManagementService : IDirectApiClientManagementService
    {
        public DirectApiClientManagementService(IHttpClientFactory httpClientFactory, IAgentService agentService)
        {
            _httpClientFactory = httpClientFactory;
            _agentService = agentService;

        }


        public async Task<List<DirectApiClient>> GetAllClients()
        {
            using var client = _httpClientFactory.CreateClient(HttpClientNames.DacManagementClient);
            var response = await client.GetAsync("direct-api-clients");

            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<List<DirectApiClient>>()
                : new List<DirectApiClient>(0);
        }


        public async Task<Result<DirectApiClient>> GetById(string clientId)
        {
            using var client = _httpClientFactory.CreateClient(HttpClientNames.DacManagementClient);
            var response = await client.GetAsync($"direct-api-clients/{clientId}");

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<DirectApiClient>();

            var error = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            return Result.Failure<DirectApiClient>(error.Detail);
        }


        public async Task<Result> Create(DirectApiClient apiClient)
        {
            using var client = _httpClientFactory.CreateClient(HttpClientNames.DacManagementClient);
            var response = await client.PostAsJsonAsync("direct-api-clients", apiClient);

            if (response.IsSuccessStatusCode)
                return Result.Success();

            var error = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            return Result.Failure<DirectApiClient>(error.Detail);
        }


        public async Task<Result> Delete(string clientId)
        {
            using var client = _httpClientFactory.CreateClient(HttpClientNames.DacManagementClient);
            var response = await client.DeleteAsync($"direct-api-clients/{clientId}");

            if (response.IsSuccessStatusCode)
                return Result.Success();

            var error = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            return Result.Failure<DirectApiClient>(error.Detail);
        }


        public async Task<Result> Activate(string clientId)
        {
            using var client = _httpClientFactory.CreateClient(HttpClientNames.DacManagementClient);
            var response = await client.PostAsync($"direct-api-clients/{clientId}/activate", null);

            if (response.IsSuccessStatusCode)
                return Result.Success();

            var error = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            return Result.Failure<DirectApiClient>(error.Detail);
        }


        public async Task<Result> Deactivate(string clientId)
        {
            using var client = _httpClientFactory.CreateClient(HttpClientNames.DacManagementClient);
            var response = await client.PostAsync($"direct-api-clients/{clientId}/deactivate", null);

            if (response.IsSuccessStatusCode)
                return Result.Success();

            var error = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            return Result.Failure<DirectApiClient>(error.Detail);
        }


        public async Task<Result> BindToAgent(string clientId, int agentId)
        {
            return await IsDacExists(clientId)
                .Bind(BindDacToAgent);


            Task<Result> BindDacToAgent() 
                => _agentService.BindDirectApiClient(agentId, clientId);
        }


        public async Task<Result> UnbindFromAgent(string clientId, int agentId)
        {
            return await IsDacExists(clientId)
                .Bind(UnbindDacFromAgent);


            Task<Result> UnbindDacFromAgent() 
                => _agentService.UnbindDirectApiClient(agentId, clientId);
        }


        private async Task<Result> IsDacExists(string clientId)
        {
            using var client = _httpClientFactory.CreateClient(HttpClientNames.DacManagementClient);
            var response = await client.GetAsync($"direct-api-clients/{clientId}");

            return response.IsSuccessStatusCode
                ? Result.Success()
                : Result.Failure($"Direct api client with Id `{clientId}` not found");
        }


        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAgentService _agentService;
    }
}