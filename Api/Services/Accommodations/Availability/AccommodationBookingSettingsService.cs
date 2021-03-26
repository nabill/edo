using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.Edo.Data.Agents;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public class AccommodationBookingSettingsService : IAccommodationBookingSettingsService
    {
        public AccommodationBookingSettingsService(IDoubleFlow doubleFlow,
            IAgentSystemSettingsService agentSystemSettingsService,
            IAgencySystemSettingsService agencySystemSettingsService,
            ICounterpartySystemSettingsService counterpartySystemSettingsService,
            IOptions<SupplierOptions> supplierOptions)
        {
            _doubleFlow = doubleFlow;
            _agentSystemSettingsService = agentSystemSettingsService;
            _supplierOptions = supplierOptions.Value;
            _agencySystemSettingsService = agencySystemSettingsService;
            _counterpartySystemSettingsService = counterpartySystemSettingsService;
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
                var counterpartySettings = await _counterpartySystemSettingsService.GetAccommodationBookingSettings(agent.CounterpartyId);

                return MergeSettings(agentSettings, agencySettings, counterpartySettings);
            }, SettingsCacheLifetime);
        }


        private AccommodationBookingSettings MergeSettings(Maybe<AgentAccommodationBookingSettings> agentSettings,
            Maybe<AgencyAccommodationBookingSettings> agencySettings, CounterpartyAccommodationBookingSettings counterpartySettings)
        {
            List<Suppliers> enabledConnectors = default;
            AprMode? aprMode = default;
            PassedDeadlineOffersMode? passedDeadlineOffersMode = default;
            bool isMarkupDisabled = default;
            bool isSupplierVisible = default;
            bool areTagsVisible = default;
            bool canSearchOnlyDirectContracts = default;
            
            if (agentSettings.HasValue)
                SetValuesFromAgentSettings(agentSettings.Value);
            
            if (agencySettings.HasValue)
                SetValuesFromAgencySettings(agencySettings.Value);

            enabledConnectors ??= _supplierOptions.EnabledSuppliers;
            aprMode ??= DefaultAprMode;
            passedDeadlineOffersMode ??= DefaultPassedDeadlineOffersMode;
            
            return new AccommodationBookingSettings(enabledConnectors,
                aprMode.Value,
                passedDeadlineOffersMode.Value,
                isMarkupDisabled, 
                isSupplierVisible,
                counterpartySettings.CancellationPolicyProcessSettings,
                areTagsVisible,
                canSearchOnlyDirectContracts);


            void SetValuesFromAgentSettings(AgentAccommodationBookingSettings agentSettingsValue)
            {
                enabledConnectors = agentSettingsValue.EnabledSuppliers;
                aprMode = agentSettingsValue.AprMode;
                passedDeadlineOffersMode = agentSettingsValue.PassedDeadlineOffersMode;
                isMarkupDisabled = agentSettingsValue.IsMarkupDisabled;
                isSupplierVisible = agentSettingsValue.IsSupplierVisible;
                areTagsVisible = agentSettingsValue.AreTagsVisible;
                canSearchOnlyDirectContracts = agentSettingsValue.CanSearchOnlyDirectContracts;
            }


            void SetValuesFromAgencySettings(AgencyAccommodationBookingSettings agencySettingsValue)
            {
                enabledConnectors ??= agencySettingsValue.EnabledSuppliers;
                aprMode ??= agencySettingsValue.AprMode;
                passedDeadlineOffersMode ??= agencySettingsValue.PassedDeadlineOffersMode;
                isMarkupDisabled = isMarkupDisabled || agencySettingsValue.IsMarkupDisabled;
                isSupplierVisible = isSupplierVisible || agencySettingsValue.IsSupplierVisible;
                areTagsVisible = areTagsVisible || agencySettingsValue.AreTagsVisible;
                canSearchOnlyDirectContracts = canSearchOnlyDirectContracts || agencySettingsValue.CanSearchOnlyDirectContracts;
            }
        }

        
        private const PassedDeadlineOffersMode DefaultPassedDeadlineOffersMode = PassedDeadlineOffersMode.DisplayOnly;

        private const AprMode DefaultAprMode = AprMode.DisplayOnly;
        
        private readonly IDoubleFlow _doubleFlow;
        private readonly IAgentSystemSettingsService _agentSystemSettingsService;
        private readonly IAgencySystemSettingsService _agencySystemSettingsService;
        private readonly SupplierOptions _supplierOptions;
        
        private static readonly TimeSpan SettingsCacheLifetime = TimeSpan.FromMinutes(5);
        private readonly ICounterpartySystemSettingsService _counterpartySystemSettingsService;
    }
}