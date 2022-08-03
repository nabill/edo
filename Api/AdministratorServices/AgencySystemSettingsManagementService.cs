using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.AdministratorServices;
using Api.Services.Internal;
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
using HappyTravel.Money.Enums;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class AgencySystemSettingsManagementService : IAgencySystemSettingsManagementService
    {
        public AgencySystemSettingsManagementService(EdoContext context,
            IManagementAuditService managementAuditService,
            ICompanyInfoService companyInfoService,
            IInternalSystemSettingsService internalSystemSettingsService)
        {
            _context = context;
            _managementAuditService = managementAuditService;
            _companyInfoService = companyInfoService;
            _internalSystemSettingsService = internalSystemSettingsService;
        }


        public Task<Result<AgencyAccommodationBookingSettings>> GetAvailabilitySearchSettings(int agencyId)
            => _internalSystemSettingsService.GetAgencyMaterializedSearchSettings(agencyId);


        public AgencyAccommodationBookingSettings GetAvailabilitySearchSettings(ContractKind? contractKind, AgencyAccommodationBookingSettings? rootSettings, AgencyAccommodationBookingSettings? agencySettings)
            => _internalSystemSettingsService.GetAgencyMaterializedSearchSettings(contractKind, rootSettings, agencySettings);


        public async Task<Result> SetAvailabilitySearchSettings(int agencyId, AgencyAccommodationBookingSettingsInfo settings)
        {
            var agency = await _context.Agencies.SingleOrDefaultAsync(a => a.Id == agencyId);

            if (agency == default)
                return Result.Failure("Agency doesn't exist");

            return await Validate()
                .BindWithTransaction(_context, () => SetSettings()
                    .Bind(WriteToAuditLog));


            async Task<Result> Validate()
            {
                var availableCurrencies = new List<Currencies>();
                var (_, isFailure, companyInfo) = await _companyInfoService.Get();
                if (!isFailure)
                    availableCurrencies = companyInfo.AvailableCurrencies;

                if (!agency.IsActive)
                    return Result.Failure("Agency is not active");

                if (agency.ContractKind is ContractKind.OfflineOrCreditCardPayments && settings.AprMode is AprMode.CardAndAccountPurchases)
                    return Result.Failure("For an agency with contract type OfflineOrCreditCardPayments, you cannot set AprMode to CardAndAccountPurchases.");

                if (agency.ContractKind is ContractKind.OfflineOrCreditCardPayments && settings.PassedDeadlineOffersMode is PassedDeadlineOffersMode.CardAndAccountPurchases)
                    return Result.Failure("For an agency with contract type OfflineOrCreditCardPayments, you cannot set PassedDeadlineOffersMode to CardAndAccountPurchases.");

                if (!await CheckFullyVerified())
                    return Result.Failure("Changing settings for agency without full access is not allowed");

                if (settings.AvailableCurrencies.Except(availableCurrencies).Any())
                    return Result.Failure($"Request's availability currencies contain not allowed currencies! Allowed currencies: {String.Join(", ", availableCurrencies.ToArray())}");

                return Result.Success();
            }


            async Task<Result> SetSettings()
            {
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


            async Task<bool> CheckFullyVerified()
            {
                if (agency.ParentId is null)
                    return agency.VerificationState is AgencyVerificationStates.FullAccess;

                var parentAgency = await _context.Agencies.SingleOrDefaultAsync(a => a.Id == agency.ParentId);
                return parentAgency.VerificationState is AgencyVerificationStates.FullAccess;
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
        private readonly ICompanyInfoService _companyInfoService;
        private readonly IInternalSystemSettingsService _internalSystemSettingsService;
    }
}