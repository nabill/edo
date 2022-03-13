using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.SupplierOptionsProvider;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public class AccommodationBookingSettingsService : IAccommodationBookingSettingsService
    {
        public AccommodationBookingSettingsService(IDoubleFlow doubleFlow,
            IAgentSystemSettingsService agentSystemSettingsService,
            IAgencySystemSettingsService agencySystemSettingsService,
            IRootAgencySystemSettingsService rootAgencySystemSettingsService,
            ISupplierOptionsStorage supplierOptionsStorage
            )
        {
            _doubleFlow = doubleFlow;
            _agentSystemSettingsService = agentSystemSettingsService;
            _agencySystemSettingsService = agencySystemSettingsService;
            _rootAgencySystemSettingsService = rootAgencySystemSettingsService;
            _supplierOptionsStorage = supplierOptionsStorage;
        }


        public Task<AccommodationBookingSettings> Get(AgentContext agent)
        {
            var key = _doubleFlow.BuildKey(nameof(AccommodationBookingSettingsService),
                nameof(Get),
                agent.AgentId.ToString(),
                agent.AgencyId.ToString());

            return _doubleFlow.GetOrSetAsync(key, async () =>
            {
                var agentSettings = await _agentSystemSettingsService.GetAccommodationBookingSettings(agent);
                var agencySettings = await _agencySystemSettingsService.GetAccommodationBookingSettings(agent.AgencyId);
                var rootAgencySettings = await _rootAgencySystemSettingsService.GetAccommodationBookingSettings(agent.AgencyId);

                return MergeSettings(agentSettings, agencySettings, rootAgencySettings);
            }, SettingsCacheLifetime);
        }


        private AccommodationBookingSettings MergeSettings(Maybe<AgentAccommodationBookingSettings> agentSettings,
            Maybe<AgencyAccommodationBookingSettings> agencySettings, RootAgencyAccommodationBookingSettings rootAgencySettings)
        {
            var agentSettingsValue = agentSettings.HasValue 
                ? agentSettings.Value
                : null;
            var agencySettingsValue = agencySettings.HasValue 
                ? agencySettings.Value
                : null;

            List<string> enabledConnectors = agentSettingsValue?.EnabledSuppliers ?? agencySettingsValue?.EnabledSuppliers ?? GetEnabledConnectors();
            AprMode? aprMode = agentSettingsValue?.AprMode ?? agencySettingsValue?.AprMode ?? DefaultAprMode;
            PassedDeadlineOffersMode? passedDeadlineOffersMode = agentSettingsValue?.PassedDeadlineOffersMode ?? agencySettingsValue?.PassedDeadlineOffersMode ??
                DefaultPassedDeadlineOffersMode;

            bool isSupplierVisible = agentSettingsValue?.IsSupplierVisible == true || agencySettingsValue?.IsSupplierVisible == true;
            bool isDirectContractFlagVisible = agentSettingsValue?.IsDirectContractFlagVisible == true || agencySettingsValue?.IsDirectContractFlagVisible == true;

            SearchFilters additionalSearchFilters = agentSettingsValue?.AdditionalSearchFilters ?? default;

            var cancellationPolicyProcessSettings = rootAgencySettings.CancellationPolicyProcessSettings;

            if (agencySettings.HasValue && agencySettings.Value.CustomDeadlineShift.HasValue)
            {
                cancellationPolicyProcessSettings = new CancellationPolicyProcessSettings
                {
                    PolicyStartDateShift = TimeSpan.FromDays(agencySettings.Value.CustomDeadlineShift.Value)
                };
            }

            return new AccommodationBookingSettings(enabledConnectors: enabledConnectors,
                aprMode: aprMode.Value,
                passedDeadlineOffersMode: passedDeadlineOffersMode.Value,
                isSupplierVisible: isSupplierVisible,
                cancellationPolicyProcessSettings: cancellationPolicyProcessSettings,
                isDirectContractFlagVisible: isDirectContractFlagVisible,
                additionalSearchFilters: additionalSearchFilters);
        }


        private List<string> GetEnabledConnectors()
        {
            var (_, isFailure, suppliers, _) = _supplierOptionsStorage.GetAll();
            if (isFailure)
                return new List<string>(0);
            
            return suppliers.Where(s => s.IsEnabled)
                .Select(x => x.Code)
                .ToList();
        }


        private const PassedDeadlineOffersMode DefaultPassedDeadlineOffersMode = PassedDeadlineOffersMode.DisplayOnly;

        private const AprMode DefaultAprMode = AprMode.DisplayOnly;
        
        private readonly IDoubleFlow _doubleFlow;
        private readonly IAgentSystemSettingsService _agentSystemSettingsService;
        private readonly IAgencySystemSettingsService _agencySystemSettingsService;
        private readonly ISupplierOptionsStorage _supplierOptionsStorage;
        
        private static readonly TimeSpan SettingsCacheLifetime = TimeSpan.FromMinutes(5);
        private readonly IRootAgencySystemSettingsService _rootAgencySystemSettingsService;
    }
}