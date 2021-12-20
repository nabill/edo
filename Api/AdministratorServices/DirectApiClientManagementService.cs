using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class DirectApiClientManagementService : IDirectApiClientManagementService
    {
        public DirectApiClientManagementService(IHttpClientFactory httpClientFactory, EdoContext context)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
        }


        public async Task<Result> AddApiClient(CreateDirectApiClientRequest request)
        {
            return await Validate()
                .Bind(CreateDirectApiClient)
                .Tap(BindToAgent);


            async Task<Result> Validate()
            {
                var isAgentExists = await _context.Agents
                    .AnyAsync(a => a.Id == request.AgentId);

                if (!isAgentExists)
                    return Result.Failure($"Agent with id {request.AgentId} not found in agency {request.AgencyId}");

                var isClientAlreadyBounded = await _context.AgentDirectApiClientRelations
                    .AnyAsync(r => r.DirectApiClientId == request.ClientId);

                if (isClientAlreadyBounded)
                    return Result.Failure($"Client with id {request.ClientId} already bounded");

                return Result.Success();
            }


            Task<Result> CreateDirectApiClient()
            {
                return Send(new HttpRequestMessage(HttpMethod.Post, "api/direct-api-clients")
                {
                    Content = new StringContent(JsonSerializer.Serialize(new
                    {
                        Id = request.ClientId,
                        request.Password,
                        request.Name
                    }), Encoding.UTF8, "application/json")
                });
            }


            async Task BindToAgent()
            {
                _context.AgentDirectApiClientRelations.Add(new AgentDirectApiClientRelation
                {
                    AgentId = request.AgentId,
                    AgencyId = request.AgencyId,
                    DirectApiClientId = request.ClientId
                });
                await _context.SaveChangesAsync();
            }
        }


        public async Task<Result> RemoveApiClient(RemoveDirectApiClientRequest request)
        {
            var relation = await _context.AgentDirectApiClientRelations
                .SingleOrDefaultAsync(r => r.AgentId == request.AgentId && r.AgencyId == request.AgencyId && r.DirectApiClientId == request.ClientId);

            if (relation is null)
                return Result.Failure<AgentDirectApiClientRelation>($"Relation between agent {request.AgentId} from agency {request.AgencyId}" +
                    $" and {request.ClientId} not found");

            
            return await RemoveApiClient()
                .Tap(UnbindAgent);


            Task<Result> RemoveApiClient()
            {
                return Send(new HttpRequestMessage(HttpMethod.Delete, $"api/direct-api-clients/{request.ClientId}"));
            }


            async Task UnbindAgent()
            {
                _context.Remove(relation);
                await _context.SaveChangesAsync();
            }
        }


        public Task<Result> ChangePassword(string clientId, string password)
        {
            return Send(new HttpRequestMessage(HttpMethod.Put, $"api/direct-api-clients/{clientId}")
            {
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    Password = password
                }), Encoding.UTF8, "application/json")
            });
        }


        private async Task<Result> Send(HttpRequestMessage request)
        {
            using var client = _httpClientFactory.CreateClient(HttpClientNames.DacManagementClient);
            HttpResponseMessage response = null;

            try
            {
                response = await client.SendAsync(request);
                var stream = await response.Content.ReadAsStreamAsync();

                if (response.IsSuccessStatusCode)
                    return Result.Success();

                var error = await JsonSerializer.DeserializeAsync<ProblemDetails>(stream);
                return Result.Failure(error.Detail);

            }
            catch (JsonException ex)
            {
                var content = await response?.Content?.ReadAsStringAsync() ?? string.Empty;
                return Result.Failure($"Cannot deserialize response with error: `{ex.Message}`. Response: {content}");
            }
            catch (Exception ex)
            {
                return Result.Failure($"Request failed with error: `{ex.Message}`");
            }
            finally
            {
                response?.Dispose();
            }
        }


        private readonly IHttpClientFactory _httpClientFactory;
        private readonly EdoContext _context;
    }
}