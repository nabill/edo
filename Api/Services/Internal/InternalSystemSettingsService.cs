using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Money.Enums;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Internal
{
    public class InternalSystemSettingsService : IInternalSystemSettingsService
    {
        public InternalSystemSettingsService(EdoContext context)
        {
            _context = context;
        }


        public async Task<Result<AgencyAccommodationBookingSettings>> GetAgencyMaterializedSearchSettings(int agencyId)
        {
            var agency = await _context.Agencies.SingleOrDefaultAsync(a => a.Id == agencyId);
            if (agency is null)
                return Result.Failure<AgencyAccommodationBookingSettings>("No agency exist");

            if (agency.ContractKind is null)
                return GetDefaults();

            var rootAgencyId = agency.Ancestors.Any() ? agency.Ancestors.First() : agency.Id;

            var agencySettings = await GetSettings(agencyId);
            var rootSettings = rootAgencyId != agencyId ? await GetSettings(rootAgencyId) : agencySettings;

            return GetAgencyMaterializedSearchSettings(agency.ContractKind, rootSettings, agencySettings);


            async Task<AgencyAccommodationBookingSettings?> GetSettings(int id)
                => (await _context.AgencySystemSettings.SingleOrDefaultAsync(s => s.AgencyId == id))?.AccommodationBookingSettings;


            AgencyAccommodationBookingSettings GetDefaults()
                => new()
                {
                    IsSupplierVisible = false,
                    IsDirectContractFlagVisible = false,
                    AprMode = AprMode.Hide,
                    PassedDeadlineOffersMode = PassedDeadlineOffersMode.Hide,
                    CustomDeadlineShift = 0
                };
        }


        public AgencyAccommodationBookingSettings GetAgencyMaterializedSearchSettings(ContractKind? contractKind, AgencyAccommodationBookingSettings? rootSettings, AgencyAccommodationBookingSettings? agencySettings)
        {
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

                if (contractKind is ContractKind.OfflineOrCreditCardPayments && agencySettings.AprMode.Value is AprMode.CardAndAccountPurchases)
                    return AprMode.CardPurchasesOnly;

                return agencySettings.AprMode.Value;
            }


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

                if (contractKind is ContractKind.OfflineOrCreditCardPayments &&
                    agencySettings.PassedDeadlineOffersMode.Value is PassedDeadlineOffersMode.CardAndAccountPurchases)
                    return PassedDeadlineOffersMode.CardPurchasesOnly;

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


        public Task<Result<AgentAccommodationBookingSettings>> GetAgentMaterializedSearchSettings(int agentId, int agencyId)
            => CheckRelationExists(agentId, agencyId)
                .Map(() => GetSettings(agentId, agencyId));


        public async Task<Result> CheckRelationExists(int agentId, int agencyId)
            => await _context.AgentAgencyRelations.AnyAsync(r => r.AgentId == agentId && r.AgencyId == agencyId)
                ? Result.Success()
                : Result.Failure("Could not find specified agent in given agency");


        private async Task<AgentAccommodationBookingSettings> GetSettings(int agentId, int agencyId)
        {
            var (_, isAgencySettingsFailure, materializedAgencySettings) = await GetAgencyMaterializedSearchSettings(agencyId);
            var agentSettings = (await _context.AgentSystemSettings.SingleOrDefaultAsync(s => s.AgentId == agentId && s.AgencyId == agencyId))
                ?.AccommodationBookingSettings;

            if (agentSettings is null)
            {
                if (isAgencySettingsFailure)
                    return new AgentAccommodationBookingSettings
                    {
                        AdditionalSearchFilters = new(),
                        AprMode = AprMode.Hide,
                        CustomDeadlineShift = 0,
                        IsDirectContractFlagVisible = false,
                        IsSupplierVisible = false,
                        PassedDeadlineOffersMode = PassedDeadlineOffersMode.Hide
                    };

                return new AgentAccommodationBookingSettings
                {
                    AdditionalSearchFilters = new(),
                    AprMode = materializedAgencySettings.AprMode,
                    CustomDeadlineShift = materializedAgencySettings.CustomDeadlineShift,
                    IsDirectContractFlagVisible = false,
                    IsSupplierVisible = false,
                    PassedDeadlineOffersMode = materializedAgencySettings.PassedDeadlineOffersMode
                };
            }

            var aprMode = agentSettings.AprMode > materializedAgencySettings.AprMode
                ? materializedAgencySettings.AprMode
                : agentSettings.AprMode;

            var passedDeadlineOffersMode = agentSettings.PassedDeadlineOffersMode > materializedAgencySettings.PassedDeadlineOffersMode
                ? materializedAgencySettings.PassedDeadlineOffersMode
                : agentSettings.PassedDeadlineOffersMode;

            var isDirectContractFlagVisible = agentSettings.IsDirectContractFlagVisible && materializedAgencySettings.IsDirectContractFlagVisible;
            var isSupplierVisible = agentSettings.IsSupplierVisible && materializedAgencySettings.IsSupplierVisible;

            return new AgentAccommodationBookingSettings
            {
                AdditionalSearchFilters = agentSettings.AdditionalSearchFilters,
                AprMode = aprMode,
                CustomDeadlineShift = agentSettings.CustomDeadlineShift,
                IsDirectContractFlagVisible = isDirectContractFlagVisible,
                IsSupplierVisible = isSupplierVisible,
                PassedDeadlineOffersMode = passedDeadlineOffersMode
            };
        }


        private readonly EdoContext _context;
    }
}