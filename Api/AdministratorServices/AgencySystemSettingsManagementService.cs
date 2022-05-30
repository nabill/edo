using System.Linq;
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
            var agency = await _context.Agencies.SingleOrDefaultAsync(a => a.Id == agencyId);
            if (agency is null)
                return Result.Failure<AgencyAccommodationBookingSettings>("No agency exist");
            
            if (agency.ContractKind == null)
                return Result.Failure<AgencyAccommodationBookingSettings>("ContractKind for agency is not set");

            var rootAgencyId = agency.Ancestors.Any() ? agency.Ancestors.First() : agency.Id;

            var rootSettings = await GetSettings(rootAgencyId);
            var agencySettings = rootAgencyId != agencyId ? await GetSettings(agencyId) : null;

            return MergeSettings(rootSettings, agencySettings);
            
            
            async Task<AgencyAccommodationBookingSettings?> GetSettings(int id) 
                => (await _context.AgencySystemSettings.SingleOrDefaultAsync(s => s.AgencyId == id))
                    ?.AccommodationBookingSettings;


            AgencyAccommodationBookingSettings MergeSettings(AgencyAccommodationBookingSettings? rootSettings, AgencyAccommodationBookingSettings? agencySettings)
            {
                var isSupplierVisible = false;
                var isDirectContractVisible = false;
                var customDeadlineShift = 0;
                var aprMode = AprMode.Hide;
                var passedDeadlineOffersMode = PassedDeadlineOffersMode.Hide;

                if (rootSettings != null && rootSettings.IsSupplierVisible 
                    && (agencySettings == null || agencySettings.IsSupplierVisible))
                {
                    isSupplierVisible = true;
                }
                
                if (rootSettings != null && rootSettings.IsDirectContractFlagVisible 
                    && (agencySettings == null || agencySettings.IsDirectContractFlagVisible))
                {
                    isDirectContractVisible = true;
                }
                
                if (agencySettings != null && rootSettings != null)
                {
                    if (agencySettings.AprMode != null && rootSettings.AprMode != null 
                        && agencySettings.AprMode > rootSettings.AprMode)
                        aprMode = rootSettings.AprMode.Value;

                    if (agencySettings.PassedDeadlineOffersMode != null && rootSettings.PassedDeadlineOffersMode != null
                        && agencySettings.PassedDeadlineOffersMode > rootSettings.PassedDeadlineOffersMode)
                        passedDeadlineOffersMode = rootSettings.PassedDeadlineOffersMode.Value;
                }

                if (agencySettings != null && agencySettings.CustomDeadlineShift != null)
                    customDeadlineShift = agencySettings.CustomDeadlineShift.Value;

                return new AgencyAccommodationBookingSettings
                {
                    AprMode = aprMode,
                    PassedDeadlineOffersMode = passedDeadlineOffersMode,
                    IsSupplierVisible = isSupplierVisible,
                    IsDirectContractFlagVisible = isDirectContractVisible,
                    CustomDeadlineShift = customDeadlineShift
                };
            }
        }


        public async Task<Result> SetAvailabilitySearchSettings(int agencyId, AgencyAccommodationBookingSettingsInfo settings)
        {
            return await Validate()
                .BindWithTransaction(_context, () => SetSettings()
                    .Bind(WriteToAuditLog));


            async Task<Result> Validate()
            {
                var agency = await _context.Agencies.SingleOrDefaultAsync(a => a.Id == agencyId);

                if (agency == default)
                    return Result.Failure("Agency doesn't exist");

                if (!agency.IsActive)
                    return Result.Failure("Agency is not active");

                if (agency.ContractKind == ContractKind.OfflineOrCreditCardPayments && settings.AprMode != AprMode.Hide)
                    return Result.Failure("For an agency with contract type OfflineOrCreditCardPayments, you cannot set AprMode other than Hide.");

                return Result.Success();
            }


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
                .BindWithTransaction(_context, () => Result.Success()
                    .Tap(DeleteSettings)
                    .Bind(WriteToAuditLog));


            async Task DeleteSettings()
            {
                var existingSettings = await _context.AgencySystemSettings.SingleOrDefaultAsync(s => s.AgencyId == agencyId);
                if (existingSettings == default)
                    return;

                _context.Remove(existingSettings);
                await _context.SaveChangesAsync();
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