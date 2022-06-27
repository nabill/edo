using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.AdministratorServices;
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
            IManagementAuditService managementAuditService, ICompanyInfoService companyInfoService)
        {
            _context = context;
            _managementAuditService = managementAuditService;
            _companyInfoService = companyInfoService;
        }


        public async Task<Result<AgencyAccommodationBookingSettings>> GetAvailabilitySearchSettings(int agencyId)
        {
            var agency = await _context.Agencies.SingleOrDefaultAsync(a => a.Id == agencyId);
            if (agency is null)
                return Result.Failure<AgencyAccommodationBookingSettings>("No agency exist");

            if (agency.ContractKind is null)
                return GetDefaults();

            var rootAgencyId = agency.Ancestors.Any() ? agency.Ancestors.First() : agency.Id;

            var agencySettings = await GetSettings(agencyId);
            var rootSettings = rootAgencyId != agencyId ? await GetSettings(rootAgencyId) : agencySettings;

            return new AgencyAccommodationBookingSettings
            {
                AprMode = GetAprMode(),
                PassedDeadlineOffersMode = GetPassedDeadlineOffersMode(),
                IsSupplierVisible = GetIsSupplierVisible(),
                IsDirectContractFlagVisible = GetIsDirectContractFlagVisible(),
                CustomDeadlineShift = GetCustomDeadlineShift(),
                AvailableCurrencies = GetAvailableCurrencies()
            };


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
                => (await _context.AgencySystemSettings.SingleOrDefaultAsync(s => s.AgencyId == id))?.AccommodationBookingSettings;


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


            List<Currencies> GetAvailableCurrencies()
                => agencySettings?.AvailableCurrencies != null
                    ? agencySettings.AvailableCurrencies
                    : new List<Currencies>();
        }


        public async Task<Result> SetAvailabilitySearchSettings(int agencyId, AgencyAccommodationBookingSettingsInfo settings)
        {
            return await Validate()
                .BindWithTransaction(_context, () => SetSettings()
                    .Bind(WriteToAuditLog));


            async Task<Result> Validate()
            {
                var agency = await _context.Agencies.SingleOrDefaultAsync(a => a.Id == agencyId);

                var availableCurrencies = new List<Currencies>();
                var (_, IsFailure, companyInfo) = await _companyInfoService.Get();
                if (!IsFailure)
                    availableCurrencies = companyInfo.AvailableCurrencies;

                if (agency == default)
                    return Result.Failure("Agency doesn't exist");

                if (!agency.IsActive)
                    return Result.Failure("Agency is not active");

                if (agency.ContractKind == ContractKind.OfflineOrCreditCardPayments && settings.AprMode != AprMode.Hide)
                    return Result.Failure("For an agency with contract type OfflineOrCreditCardPayments, you cannot set AprMode other than Hide.");

                if (agency.VerificationState is not AgencyVerificationStates.FullAccess)
                    return Result.Failure("Changing settings for agency without full access is not allowed");

                if (settings.AvailableCurrencies.Except(availableCurrencies).Count() > 0)
                    return Result.Failure($"Request's availablity currencies contain unallowed currencies! Allowed currencies: {String.Join(", ", availableCurrencies.ToArray())}");

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
    }
}