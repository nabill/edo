using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Management.AuditEvents;
using HappyTravel.Edo.Api.Models.Settings;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class AgentSystemSettingsManagementService : IAgentSystemSettingsManagementService
    {
        public AgentSystemSettingsManagementService(EdoContext context,
            IManagementAuditService managementAuditService)
        {
            _context = context;
            _managementAuditService = managementAuditService;
        }


        public Task<Result> SetAvailabilitySearchSettings(int agentId, int agencyId, AgentAccommodationBookingSettingsInfo settings)
        {
            return CheckRelationExists(agentId, agencyId)
                .BindWithTransaction(_context, () => Result.Success()
                    .Map(() => GetOrDefault(agentId, agencyId))
                    .Tap(Update)
                    .Bind(WriteAuditLog));


             Task Update(AgentSystemSettings existingSettings)
             {
                 if (existingSettings is null)
                 {
                     var newSettings = new AgentSystemSettings
                     {
                         AgencyId = agencyId,
                         AgentId = agentId,
                         AccommodationBookingSettings = settings.ToAgentAccommodationBookingSettings()
                     };
                     _context.Add(newSettings);
                 }
                 else
                 {
                     existingSettings.AccommodationBookingSettings = settings.ToAgentAccommodationBookingSettings();
                     _context.Update(existingSettings);
                 }

                 return _context.SaveChangesAsync();
             }


             Task<Result> WriteAuditLog(AgentSystemSettings _)
                 => _managementAuditService.Write(ManagementEventType.AgentSystemSettingsCreateOrEdit,
                     new AgentSystemSettingsCreateOrEditEventData(agentId, agencyId, settings));
        }


        public Task<Result<AgentAccommodationBookingSettings>> GetAvailabilitySearchSettings(int agentId, int agencyId)
            => CheckRelationExists(agentId, agencyId)
                .Map(() => GetOrDefault(agentId, agencyId))
                .Map(s => s?.AccommodationBookingSettings);
        

        public async Task<Result> DeleteAvailabilitySearchSettings(int agentId, int agencyId)
        {
            return await CheckRelationExists(agentId, agencyId)
                .BindWithTransaction(_context, () => Result.Success()
                    .Tap(Remove)
                    .Bind(WriteAuditLog));


            async Task Remove()
            {
                var settings = await GetOrDefault(agentId, agencyId);
                if (settings is null)
                    return;

                _context.Remove(settings);
                await _context.SaveChangesAsync();
            }


            Task<Result> WriteAuditLog()
                => _managementAuditService.Write(ManagementEventType.AgentSystemSettingsDelete,
                    new AgentSystemSettingsDeleteEventData(agentId, agencyId));
        }


        private async Task<Result> CheckRelationExists(int agentId, int agencyId)
            => await _context.AgentAgencyRelations.AnyAsync(r => r.AgentId == agentId && r.AgencyId == agencyId)
                ? Result.Success()
                : Result.Failure("Could not find specified agent in given agency");


        private Task<AgentSystemSettings> GetOrDefault(int agentId, int agencyId)
            => _context.AgentSystemSettings.SingleOrDefaultAsync(s => s.AgentId == agentId && s.AgencyId == agencyId);


        private readonly EdoContext _context;
        private readonly IManagementAuditService _managementAuditService;
    }
}