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

            if (agency.ContractKind is null)
                return GetDefaults();

            var rootAgencyId = agency.Ancestors.Any() ? agency.Ancestors.First() : agency.Id;

            var rootSettings = await GetSettings(rootAgencyId);
            var agencySettings = rootAgencyId != agencyId ? await GetSettings(agencyId) : null;
            
            return new AgencyAccommodationBookingSettings
            {
                AprMode = GetAprMode(),
                PassedDeadlineOffersMode = GetPassedDeadlineOffersMode(),
                IsSupplierVisible = GetIsSupplierVisible(),
                IsDirectContractFlagVisible = GetIsDirectContractFlagVisible(),
                CustomDeadlineShift = GetCustomDeadlineShift()
            };


            AgencyAccommodationBookingSettings GetDefaults()
                => new()
                {
                    IsSupplierVisible = false,
                    IsDirectContractFlagVisible = false,
                    AprMode = AprMode.Hide,
                    PassedDeadlineOffersMode = PassedDeadlineOffersMode.Hide,
                    CustomDeadlineShift = 0
                };
            
            
            async Task<AgencyAccommodationBookingSettings?> GetSettings(int id) 
                => (await _context.AgencySystemSettings.SingleOrDefaultAsync(s => s.AgencyId == id))
                    ?.AccommodationBookingSettings;


            bool GetIsSupplierVisible()
            {
                if (rootSettings is null)
                    return false;

                if (agencySettings is null)
                    return rootSettings.IsSupplierVisible;

                return rootSettings.IsSupplierVisible && agencySettings.IsSupplierVisible;
            }
            
            
            bool GetIsDirectContractFlagVisible()
            {
                if (rootSettings is null)
                    return false;

                if (agencySettings is null)
                    return rootSettings.IsDirectContractFlagVisible;

                return rootSettings.IsDirectContractFlagVisible && agencySettings.IsDirectContractFlagVisible;
            }
            
            
            AprMode GetAprMode()
            {
                if (rootSettings?.AprMode is null)
                    return AprMode.Hide;

                if (agencySettings?.AprMode is null)
                    return rootSettings.AprMode.Value;

                if (rootSettings.AprMode < agencySettings.AprMode)
                    return rootSettings.AprMode.Value;

                return agencySettings.AprMode.Value;
            }

            
            PassedDeadlineOffersMode GetPassedDeadlineOffersMode()
            {
                if (rootSettings?.PassedDeadlineOffersMode is null)
                    return PassedDeadlineOffersMode.Hide;

                if (agencySettings?.PassedDeadlineOffersMode is null)
                    return rootSettings.PassedDeadlineOffersMode.Value;

                if (rootSettings.PassedDeadlineOffersMode < agencySettings.PassedDeadlineOffersMode)
                    return rootSettings.PassedDeadlineOffersMode.Value;

                return agencySettings.PassedDeadlineOffersMode.Value;
            }
            
            int GetCustomDeadlineShift() 
                => (agencySettings != null && agencySettings.CustomDeadlineShift != null)
                    ? agencySettings.CustomDeadlineShift.Value
                    : 0;
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
                
                if (agency.ContractKind is null)
                    return Result.Failure("Changing settings for agency in read-only mode is not allowed");

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