using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Notifications.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Management
{
    /// <summary>
    /// A service for managing direct api clients
    /// </summary>
    public class DirectApiClientManagementService : IDirectApiClientManagementService
    {
        public DirectApiClientManagementService(IHttpClientFactory httpClientFactory, EdoContext context, 
            INotificationService notificationsService)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
            _notificationsService = notificationsService;
        }

        /// <summary>
        /// Creates new or update existing direct api client
        /// </summary>
        /// <param name="agent">Agent context</param>
        /// <returns></returns>
        public async Task<Result<ApiClientData>> Generate(AgentContext agent)
        {
            var name = $"{GenericDirectApiClientName}{agent.AgencyId}-{agent.AgentId}";
            var password = PasswordGenerator.Generate();
            var clientData = new ApiClientData(name, password);

            var (_, isFailure, error) = await Validate()
                .Bind(CreateDirectApiClient)
                .Bind(SendNotification);
            
            return isFailure
                ? Result.Failure<ApiClientData>(error)
                : clientData;

            Task<Result> SendNotification()
            {
                return _notificationsService.Send(
                    new ApiConnectionData(
                        agent.AgencyId.ToString(),
                        agent.AgencyName,
                        agent.AgentId.ToString(),
                        agent.AgentName), 
                    NotificationTypes.ApiConnectionCredentialsCreated);
            }


            async Task<Result> Validate()
            {
                var doesAgencyExist = await _context.AgentAgencyRelations
                    .AnyAsync(r => r.AgencyId == agent.AgencyId && r.AgentId == agent.AgentId);

                return doesAgencyExist
                    ? Result.Success()
                    : Result.Failure("Could not find agent and agency");
            }


            async Task<Result> CreateDirectApiClient()
            {
                var existingClient = await _context.AgentDirectApiClientRelations
                    .SingleOrDefaultAsync(a => a.AgentId == agent.AgentId && a.AgencyId == agent.AgencyId);

                if (existingClient is null)
                {
                    await Send(new HttpRequestMessage(HttpMethod.Post, "api/direct-api-clients")
                    {
                        Content = new StringContent(JsonSerializer.Serialize(new
                        {
                            Id = name,
                            password,
                            name
                        }), Encoding.UTF8, "application/json")
                    });
                    
                    _context.AgentDirectApiClientRelations.Add(new AgentDirectApiClientRelation
                    {
                        AgentId = agent.AgentId,
                        AgencyId = agent.AgencyId,
                        DirectApiClientId = name
                    });
                    await _context.SaveChangesAsync();
                    return Result.Success();
                }
                
                return await ChangePassword(name, password);
            }
        }
        
        
        /// <summary>
        /// Removes existing direct api client
        /// </summary>
        /// <param name="request">Direct api client parameters</param>
        /// <returns></returns>
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
        
        
        private Task<Result> ChangePassword(string clientId, string password)
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
                var content = await response.Content.ReadAsStringAsync() ?? string.Empty;
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
        
        
        private const string GenericDirectApiClientName = "DirectApiClient";


        private readonly IHttpClientFactory _httpClientFactory;
        private readonly EdoContext _context;
        private readonly INotificationService _notificationsService;
    }
}