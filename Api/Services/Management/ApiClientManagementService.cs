using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Models.Management.AuditEvents;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Notifications.Enums;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Management
{
    public class ApiClientManagementService : IApiClientManagementService
    {
        public ApiClientManagementService(
            EdoContext context,
            IManagementAuditService managementAuditService,
            INotificationService notificationsService)
        {
            _context = context;
            _managementAuditService = managementAuditService;
            _notificationsService = notificationsService;
        }


        public Task<Result> Set(int agencyId, int agentId, ApiClientData clientData)
        {
            return Validate()
                .BindWithTransaction(_context, () => SetClient()
                    .Bind(WriteAuditLog));


            async Task<Result> Validate()
            {
                var doesAgencyExist = await _context.AgentAgencyRelations
                    .AnyAsync(r => r.AgencyId == agencyId && r.AgentId == agentId);

                return doesAgencyExist
                    ? Result.Success()
                    : Result.Failure("Could not find agent and agency");
            }


            async Task<Result> SetClient()
            {
                var existingClient = await _context.ApiClients
                    .SingleOrDefaultAsync(a => a.AgentId == agentId && a.AgencyId == agencyId);

                if (existingClient is null)
                {
                    var doesSameNameClientExist = await _context.ApiClients.AnyAsync(a => a.Name == clientData.Name);
                    if (doesSameNameClientExist)
                        return Result.Failure("Client with same name already exists");

                    _context.ApiClients.Add(new ApiClient
                    {
                        AgentId = agentId,
                        AgencyId = agencyId,
                        Name = clientData.Name,
                        PasswordHash = HashGenerator.ComputeSha256(clientData.Password)
                    });
                }
                else
                {
                    existingClient.Name = clientData.Name;
                    existingClient.PasswordHash = HashGenerator.ComputeSha256(clientData.Password);
                    _context.Update(existingClient);
                }

                await _context.SaveChangesAsync();
                return Result.Success();
            }


            Task<Result> WriteAuditLog()
                => _managementAuditService.Write(ManagementEventType.AgentApiClientCreateOrEdit, new AgentApiClientEventData(agentId, agencyId));
        }


        public async Task<Result> Delete(int agencyId, int agentId)
        {
            return await GetClient()
                .BindWithTransaction(_context, c => Result.Success(c)
                    .Tap(Remove)
                    .Bind(WriteAuditLog));


            async Task<Result<ApiClient>> GetClient()
            {
                var apiClient = await _context.ApiClients
                    .SingleOrDefaultAsync(a => a.AgentId == agentId && a.AgencyId == agencyId);

                return apiClient ?? Result.Failure<ApiClient>($"Could not find api client");
            }


            Task Remove(ApiClient client)
            {
                _context.Remove(client);
                return _context.SaveChangesAsync();
            }


            Task<Result> WriteAuditLog(ApiClient _)
                => _managementAuditService.Write(ManagementEventType.AgentApiClientDelete, new AgentApiClientEventData(agentId, agencyId));
        }


        public async Task<Result<ApiClientData>> Generate(int agencyId, int agentId)
        {
            var name = $"{GenericApiClientName}{agencyId}-{agentId}";
            var password = PasswordGenerator.Generate();
            var clientData = new ApiClientData(name, password);

            var (_, isFailure, error) =
                await Set(agencyId, agentId, clientData)
                    .Bind(GetAgencyAndAgent)
                    .Bind(SendNotification);

            return isFailure
                ? Result.Failure<ApiClientData>(error)
                : clientData;


            Task<Result> SendNotification((Agency, Agent) agencyAndAgent)
            {
                var (agency, agent) = agencyAndAgent;
                return _notificationsService.Send(new ApiConnectionData(agency, agent), NotificationTypes.ApiConnectionCredentialsCreated);
            }


            async Task<Result<(Agency, Agent)>> GetAgencyAndAgent()
            {
                var agencyTask = _context.Agencies.FirstOrDefaultAsync(ag => ag.Id == agencyId);
                var agentTask = _context.Agents.FirstOrDefaultAsync(ag => ag.Id == agentId);

                await Task.WhenAll(agencyTask, agentTask);

                var agency = await agencyTask;
                var agent = await agentTask;

                if (agency == null)
                    return Result.Failure<(Agency, Agent)>("Could not find agency with specified id");
                if (agent == null)
                    return Result.Failure<(Agency, Agent)>("Could not find agent with specified id");

                return Result.Success((agency, agent));
            }
        }


        private const string GenericApiClientName = "ApiClient";

        private readonly EdoContext _context;
        private readonly IManagementAuditService _managementAuditService;
        private readonly INotificationService _notificationsService;
    }
}