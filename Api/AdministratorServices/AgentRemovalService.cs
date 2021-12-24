using System;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data;
using IdentityModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Management.AuditEvents;
using HappyTravel.Edo.Api.Services.Management;
using Microsoft.AspNetCore.Mvc;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class AgentRemovalService : IAgentRemovalService
    {
        public AgentRemovalService(EdoContext context,
            IAgentSystemSettingsManagementService agentSystemSettingsManagementService,
            IDirectApiClientManagementService apiClientManagementService,
            IHttpClientFactory httpClientFactory,
            IManagementAuditService managementAuditService)
        {
            _context = context;
            _agentSystemSettingsManagementService = agentSystemSettingsManagementService;
            _apiClientManagementService = apiClientManagementService;
            _httpClientFactory = httpClientFactory;
            _managementAuditService = managementAuditService;
        }


        public async Task<Result> RemoveFromAgency(int agentId, int agencyId)
        {
            return await Result.Success()
                .BindWithTransaction(_context, () => Result.Success()
                    .Bind(DeleteAgentSystemSettings)
                    .Bind(DeleteApiClients)
                    .Bind(DeleteAgentAgencyRelation)
                    .Bind(DeleteAgentIfRequired));


            Task<Result> DeleteAgentSystemSettings()
                => _agentSystemSettingsManagementService.DeleteAvailabilitySearchSettings(agentId, agencyId);


            async Task<Result> DeleteApiClients()
            {
                // Remove using range because no guarantees that only one client per (agent, agency)
                var apiClients = await _context.ApiClients
                    .Where(c => c.AgentId == agentId && c.AgencyId == agencyId)
                    .ToListAsync();

                if (!apiClients.Any())
                    return Result.Success();

                _context.RemoveRange(apiClients);
                await _context.SaveChangesAsync();

                return Result.Success();
            }


            async Task<Result> DeleteAgentAgencyRelation()
            {
                var relation = await _context.AgentAgencyRelations
                    .Where(r => r.AgentId == agentId && r.AgencyId == agencyId)
                    .SingleOrDefaultAsync();

                if (relation is null)
                    return Result.Failure("Agent not found in specified agency");

                _context.Remove(relation);
                await _context.SaveChangesAsync();

                return Result.Success();
            }


            async Task<Result> DeleteAgentIfRequired()
            {
                var anyOtherRelationsLeft = await _context.AgentAgencyRelations
                    .Where(r => r.AgentId == agentId && r.AgencyId != agencyId)
                    .AnyAsync();

                if (anyOtherRelationsLeft)
                    return Result.Success();

                return await Result.Success()
                    .Bind(DeleteDirectApiClients)
                    .Bind(DeleteAgent)
                    .Bind(DeleteIdentity)
                    .Tap(WriteLogAgentDeleted);

                
                async Task<Result> DeleteDirectApiClients()
                {
                    // Remove using range because no guarantees that only one API client per (agent, agency)
                    var directApiClients = await _context.AgentDirectApiClientRelations
                        .Where(r => r.AgentId == agentId && r.AgencyId == agencyId)
                        .ToListAsync();

                    foreach (var directApiClient in directApiClients)
                    {
                        var apiClientRemoveRequest = new RemoveDirectApiClientRequest(directApiClient.AgentId, directApiClient.AgencyId,
                            directApiClient.DirectApiClientId);
                        var result = await _apiClientManagementService.RemoveApiClient(apiClientRemoveRequest);

                        if (result.IsFailure)
                            return result;
                    }

                    return Result.Success();
                }


                async Task<Result<string>> DeleteAgent()
                {
                    var agent = await _context.Agents.FindAsync(agentId);
                    _context.Remove(agent);
                    await _context.SaveChangesAsync();

                    return agent.Email;
                }


                Task<Result> DeleteIdentity(string agentEmail)
                    => Send(new HttpRequestMessage(HttpMethod.Delete, $"api/users/{agentEmail}"));


                async Task<Result> Send(HttpRequestMessage request)
                {
                    using var client = _httpClientFactory.CreateClient(HttpClientNames.UsersManagementIdentityClient);
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
                        return Result.Failure($"Cannot deserialize identity response with error: `{ex.Message}`. " +
                            $"Status: {response?.StatusCode}. Response: {content}");
                    }
                    catch (Exception ex)
                    {
                        return Result.Failure($"Identity request failed with error: `{ex.Message}`");
                    }
                    finally
                    {
                        response?.Dispose();
                    }
                }


                Task WriteLogAgentDeleted()
                    => _managementAuditService.Write(ManagementEventType.AgentDeletion, new AgentDeletedEventData(agentId));
            }
        }


        private readonly EdoContext _context;
        private readonly IAgentSystemSettingsManagementService _agentSystemSettingsManagementService;
        private readonly IDirectApiClientManagementService _apiClientManagementService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IManagementAuditService _managementAuditService;
    }
}
