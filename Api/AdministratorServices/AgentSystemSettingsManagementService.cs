using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Management.AuditEvents;
using HappyTravel.Edo.Api.Models.Settings;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class AgentSystemSettingsManagementService : IAgentSystemSettingsManagementService
    {
        public AgentSystemSettingsManagementService(EdoContext context,
            IManagementAuditService managementAuditService, IAgencySystemSettingsManagementService agencyManagementService)
        {
            _context = context;
            _managementAuditService = managementAuditService;
            _agencyManagementService = agencyManagementService;
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
                .Map(() => GetSettings(agentId, agencyId));
        

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


        private async Task<AgentAccommodationBookingSettings> GetSettings(int agentId, int agencyId)
        {
            var (_, isAgencySettingsFailure, materializedAgencySettings) = await _agencyManagementService.GetAvailabilitySearchSettings(agencyId);
            var agentSettings = (await _context.AgentSystemSettings.SingleOrDefaultAsync(s => s.AgentId == agentId && s.AgencyId == agencyId))
                ?.AccommodationBookingSettings;

            if (agentSettings == null)
            {
                if (isAgencySettingsFailure)
                    return new AgentAccommodationBookingSettings
                    {
                        AdditionalSearchFilters = new(),
                        AprMode = AprMode.Hide,
                        CustomDeadlineShift = 0,
                        EnabledSuppliers = new(),
                        IsDirectContractFlagVisible = false,
                        IsSupplierVisible = false,
                        PassedDeadlineOffersMode = PassedDeadlineOffersMode.Hide
                    };

                return new AgentAccommodationBookingSettings
                {
                    AdditionalSearchFilters = new(),
                    AprMode = materializedAgencySettings.AprMode,
                    CustomDeadlineShift = materializedAgencySettings.CustomDeadlineShift,
                    EnabledSuppliers = new(),
                    IsDirectContractFlagVisible = materializedAgencySettings.IsDirectContractFlagVisible,
                    IsSupplierVisible = materializedAgencySettings.IsSupplierVisible,
                    PassedDeadlineOffersMode = materializedAgencySettings.PassedDeadlineOffersMode
                };
            }

            var aprMode = agentSettings.AprMode > materializedAgencySettings.AprMode ? materializedAgencySettings.AprMode : agentSettings.AprMode;
            var passedDeadlineOffersMode = agentSettings.PassedDeadlineOffersMode > materializedAgencySettings.PassedDeadlineOffersMode 
                ? materializedAgencySettings.PassedDeadlineOffersMode : agentSettings.PassedDeadlineOffersMode;

            var isDirectContractFlagVisible = agentSettings.IsDirectContractFlagVisible && materializedAgencySettings.IsDirectContractFlagVisible;
            var isSupplierVisible = agentSettings.IsSupplierVisible && materializedAgencySettings.IsSupplierVisible;

            return new AgentAccommodationBookingSettings
            {
                AdditionalSearchFilters = agentSettings.AdditionalSearchFilters,
                AprMode = aprMode,
                CustomDeadlineShift = agentSettings.CustomDeadlineShift,
                EnabledSuppliers = new(),
                IsDirectContractFlagVisible = isDirectContractFlagVisible,
                IsSupplierVisible = isSupplierVisible,
                PassedDeadlineOffersMode = passedDeadlineOffersMode
            };
        }


        private readonly EdoContext _context;
        private readonly IManagementAuditService _managementAuditService;
        private readonly IAgencySystemSettingsManagementService _agencyManagementService;
    }
}