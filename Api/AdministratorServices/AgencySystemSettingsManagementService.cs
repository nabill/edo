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
    public class AgencySystemSettingsManagementService : IAgencySystemSettingsManagementService
    {
        public AgencySystemSettingsManagementService(EdoContext context,
            IManagementAuditService managementAuditService)
        {
            _context = context;
            _managementAuditService = managementAuditService;
        }


        public async Task<Result<AgencyAccommodationBookingSettings>> GetAvailabilitySearchSettings(int agencyId)
        {
            return await CheckAgencyExists(agencyId)
                .Bind(GetSettings);


            async Task<Result<AgencyAccommodationBookingSettings>> GetSettings()
            {
                var existingSettings = await _context.AgencySystemSettings.SingleOrDefaultAsync(s => s.AgencyId == agencyId);
                
                return existingSettings?.AccommodationBookingSettings;
            }
        }


        public async Task<Result> SetAvailabilitySearchSettings(int agencyId, AgencyAccommodationBookingSettingsInfo settings)
        {
            return await CheckAgencyExists(agencyId)
                .BindWithTransaction(_context, () => SetSettings()
                    .Bind(WriteToAuditLog));


            async Task<Result> SetSettings()
            {
                if (settings.CustomDeadlineShift.HasValue && settings.CustomDeadlineShift >= 0) 
                    return Result.Failure("Deadline shift must be less than zero");
                
                var existingSettings = await _context.AgencySystemSettings.SingleOrDefaultAsync(s => s.AgencyId == agencyId);
                if (existingSettings == default)
                {
                    var newSettings = new AgencySystemSettings
                    {
                        AgencyId = agencyId,
                        AccommodationBookingSettings = settings.ToAgencyAccommodationBookingSettings()
                    };
                    _context.AgencySystemSettings.Add(newSettings);
                }
                else
                {
                    existingSettings.AccommodationBookingSettings = settings.ToAgencyAccommodationBookingSettings();
                    _context.Update(existingSettings);
                }

                await _context.SaveChangesAsync();
                return Result.Success();
            }


            Task<Result> WriteToAuditLog()
                => _managementAuditService.Write(ManagementEventType.AgencySystemSettingsCreateOrEdit,
                    new AgencySystemSettingsCreateOrEditEventData(agencyId, settings));
        }


        public async Task<Result> DeleteAvailabilitySearchSettings(int agencyId)
        {
            return await CheckAgencyExistsIncludingInactive(agencyId)
                .BindWithTransaction(_context, () => DeleteSettings()
                    .Bind(WriteToAuditLog));


            async Task<Result> DeleteSettings()
            {
                var existingSettings = await _context.AgencySystemSettings.SingleOrDefaultAsync(s => s.AgencyId == agencyId);
                if (existingSettings == default)
                    return Result.Success();

                _context.Remove(existingSettings);
                await _context.SaveChangesAsync();

                return Result.Success();
            }


            Task<Result> WriteToAuditLog()
                => _managementAuditService.Write(ManagementEventType.AgencySystemSettingsDelete,
                    new AgencySystemSettingsDeleteEventData(agencyId));
        }


        private async Task<Result> CheckAgencyExists(int agencyId)
            => await DoesAgencyExist(agencyId)
                ? Result.Success()
                : Result.Failure("Agency with such id does not exist");


        private async Task<Result> CheckAgencyExistsIncludingInactive(int agencyId)
            => await DoesAgencyExistIncludingInactive(agencyId)
                ? Result.Success()
                : Result.Failure("Agency with such id does not exist");


        private Task<bool> DoesAgencyExist(int agencyId)
            => _context.Agencies.AnyAsync(a => a.Id == agencyId && a.IsActive);


        private Task<bool> DoesAgencyExistIncludingInactive(int agencyId)
            => _context.Agencies.AnyAsync(a => a.Id == agencyId);


        private readonly EdoContext _context;
        private readonly IManagementAuditService _managementAuditService;
    }
}