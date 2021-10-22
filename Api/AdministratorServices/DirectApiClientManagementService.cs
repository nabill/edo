using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
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


        public Task<Result<List<DirectApiClientSlim>, ProblemDetails>> GetAllClients() 
            => Send<List<DirectApiClientSlim>>(new HttpRequestMessage(HttpMethod.Get, "api/direct-api-clients"));


        public Task<Result<DirectApiClientSlim, ProblemDetails>> GetById(string clientId) 
            => Send<DirectApiClientSlim>(new HttpRequestMessage(HttpMethod.Get, $"api/direct-api-clients/{clientId}"));


        public Task<Result<Unit, ProblemDetails>> Create(CreateDirectApiClientRequest request)
        {
            return Send<Unit>(new HttpRequestMessage(HttpMethod.Post, "api/direct-api-clients")
            {
                Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
            });
        }


        public Task<Result<Unit, ProblemDetails>> Delete(string clientId) 
            => Send<Unit>(new HttpRequestMessage(HttpMethod.Delete, $"api/direct-api-clients/{clientId}"));


        public Task<Result<Unit, ProblemDetails>> Activate(string clientId) 
            => Send<Unit>(new HttpRequestMessage(HttpMethod.Post, $"api/direct-api-clients/{clientId}/activate"));


        public Task<Result<Unit, ProblemDetails>> Deactivate(string clientId) 
            => Send<Unit>(new HttpRequestMessage(HttpMethod.Post, $"api/direct-api-clients/{clientId}/deactivate"));


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
            var response = await Send<Unit>(new HttpRequestMessage(HttpMethod.Get, $"api/direct-api-clients/{clientId}"));

            return response.IsSuccess
                ? Result.Success()
                : Result.Failure($"Direct api client with Id `{clientId}` not found");
        }


        private async Task<Result<T, ProblemDetails>> Send<T>(HttpRequestMessage request)
        {
            using var client = _httpClientFactory.CreateClient(HttpClientNames.DacManagementClient);
            HttpResponseMessage response = null;

            try
            {
                response = await client.SendAsync(request);
                var stream = await response.Content.ReadAsStreamAsync();

                if (response.IsSuccessStatusCode)
                {
                    return typeof(T) == typeof(Unit)
                        ? default
                        : await JsonSerializer.DeserializeAsync<T>(stream, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });
                }
                
                return await JsonSerializer.DeserializeAsync<ProblemDetails>(stream);

            }
            catch (JsonException ex)
            {
                var content = await response?.Content?.ReadAsStringAsync() ?? string.Empty;
                return ProblemDetailsBuilder.Fail<T>($"Cannot deserialize response with error: `{ex.Message}`. Response: {content}");
            }
            catch (Exception ex)
            {
                return ProblemDetailsBuilder.Fail<T>($"Request failed with error: `{ex.Message}`");
            }
        }


        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAgentService _agentService;
    }
}