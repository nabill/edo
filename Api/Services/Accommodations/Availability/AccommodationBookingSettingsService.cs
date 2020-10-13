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
    public class AccommodationBookingSettingsService : IAvailabilitySearchSettingsService
    {
        public AccommodationBookingSettingsService(IDoubleFlow doubleFlow,
            IAgentSystemSettingsService agentSystemSettingsService,
            IAgencySystemSettingsService agencySystemSettingsService,
            IOptions<DataProviderOptions> dataProviderOptions)
        {
            _doubleFlow = doubleFlow;
            _agentSystemSettingsService = agentSystemSettingsService;
            _dataProviderOptions = dataProviderOptions.Value;
            _agencySystemSettingsService = agencySystemSettingsService;
        }


        public Task<AccommodationBookingSettings> Get(AgentContext agent)
        {
            var key = _doubleFlow.BuildKey(nameof(AccommodationBookingSettingsService),
                nameof(Get),
                agent.AgentId.ToString(),
                agent.AgencyId.ToString());

            return _doubleFlow.GetOrSetAsync(key, async () =>
            {
                var agentSettings = await _agentSystemSettingsService.GetAvailabilitySearchSettings(agent);
                var agencySettings = await _agencySystemSettingsService.GetAvailabilitySearchSettings(agent.AgencyId);

                return MergeSettings(agentSettings, agencySettings);
            }, AvailabilitySearchSettingsCacheLifetime);
        }


        private AccommodationBookingSettings MergeSettings(Maybe<AgentAccommodationBookingSettings> agentSettings, Maybe<AgencyAccommodationBookingSettings> agencySettings)
        {
            List<DataProviders> enabledConnectors = default;
            AprMode? aprMode = default;
            PassedDeadlineOffersMode? passedDeadlineOffersMode = default;
            bool isMarkupDisabled = default;
            bool isDataProviderVisible = default;
            
            if (agentSettings.HasValue)
                SetValuesFromAgentSettings(agentSettings.Value);
            
            if (agencySettings.HasValue)
                SetValuesFromAgencySettings(agencySettings.Value);

            enabledConnectors ??= _dataProviderOptions.EnabledProviders;
            aprMode ??= DefaultAprMode;
            passedDeadlineOffersMode ??= DefaultPassedDeadlineOffersMode;
            
            return new AccommodationBookingSettings(enabledConnectors, aprMode.Value, passedDeadlineOffersMode.Value, isMarkupDisabled, isDataProviderVisible);


            void SetValuesFromAgentSettings(AgentAccommodationBookingSettings agentSettingsValue)
            {
                enabledConnectors = agentSettingsValue.EnabledProviders;
                aprMode = agentSettingsValue.AprMode;
                passedDeadlineOffersMode = agentSettingsValue.PassedDeadlineOffersMode;
                isMarkupDisabled = agentSettingsValue.IsMarkupDisabled;
                isDataProviderVisible = agentSettingsValue.IsDataProviderVisible;
            }


            void SetValuesFromAgencySettings(AgencyAccommodationBookingSettings agencySettingsValue)
            {
                enabledConnectors ??= agencySettingsValue.EnabledProviders;
                aprMode ??= agencySettingsValue.AprMode;
                passedDeadlineOffersMode ??= agencySettingsValue.PassedDeadlineOffersMode;
                isMarkupDisabled = isMarkupDisabled || agencySettingsValue.IsMarkupDisabled;
                isDataProviderVisible = isDataProviderVisible || agencySettingsValue.IsDataProviderVisible;
            }
        }

        
        private const PassedDeadlineOffersMode DefaultPassedDeadlineOffersMode = PassedDeadlineOffersMode.DisplayOnly;

        private const AprMode DefaultAprMode = AprMode.DisplayOnly;
        
        private readonly IDoubleFlow _doubleFlow;
        private readonly IAgentSystemSettingsService _agentSystemSettingsService;
        private readonly IAgencySystemSettingsService _agencySystemSettingsService;
        private readonly DataProviderOptions _dataProviderOptions;
        
        private static readonly TimeSpan AvailabilitySearchSettingsCacheLifetime = TimeSpan.FromMinutes(5);
    }
}