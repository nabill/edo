using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.SupplierOptionsProvider;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public class AccommodationBookingSettingsService : IAccommodationBookingSettingsService
    {
        public AccommodationBookingSettingsService(IDoubleFlow doubleFlow,
            IAgentSystemSettingsService agentSystemSettingsService,
            IAgencySystemSettingsService agencySystemSettingsService,
            IRootAgencySystemSettingsService rootAgencySystemSettingsService,
            ISupplierOptionsStorage supplierOptionsStorage,
            ILogger<AccommodationBookingSettingsService> logger
            )
        {
            _doubleFlow = doubleFlow;
            _agentSystemSettingsService = agentSystemSettingsService;
            _agencySystemSettingsService = agencySystemSettingsService;
            _rootAgencySystemSettingsService = rootAgencySystemSettingsService;
            _supplierOptionsStorage = supplierOptionsStorage;
            _logger = logger;
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
            
            
            AccommodationBookingSettings MergeSettings(Maybe<AgentAccommodationBookingSettings> agentSettings, Maybe<AgencyAccommodationBookingSettings> agencySettings, RootAgencyAccommodationBookingSettings rootAgencySettings)
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

                var cancellationPolicyProcessSettings =
                    MergeCancellationPolicyProcessSettings(rootAgencySettings.CancellationPolicyProcessSettings, agencySettings, agentSettings);

                return new AccommodationBookingSettings(enabledConnectors: enabledConnectors,
                    aprMode: aprMode.Value,
                    passedDeadlineOffersMode: passedDeadlineOffersMode.Value,
                    isSupplierVisible: isSupplierVisible,
                    cancellationPolicyProcessSettings: cancellationPolicyProcessSettings,
                    isDirectContractFlagVisible: isDirectContractFlagVisible,
                    additionalSearchFilters: additionalSearchFilters);
            }


            CancellationPolicyProcessSettings MergeCancellationPolicyProcessSettings(CancellationPolicyProcessSettings rootSettings,
                Maybe<AgencyAccommodationBookingSettings> agencySettings, Maybe<AgentAccommodationBookingSettings> agentSettings)
            {
                if (!agencySettings.HasValue && !agentSettings.HasValue)
                    return rootSettings;

                var shift = rootSettings.PolicyStartDateShift.Days;

                if (agencySettings.HasValue && agencySettings.Value.CustomDeadlineShift.HasValue)
                    shift += agencySettings.Value.CustomDeadlineShift.Value;

                if (agentSettings.HasValue && agentSettings.Value.CustomDeadlineShift.HasValue)
                    shift += agentSettings.Value.CustomDeadlineShift.Value;

                if (shift > 0)
                {
                    _logger.LogPositiveDeadlineShift(agent.AgentId, agent.AgencyId, shift);
                    shift = 0;
                }

                return new CancellationPolicyProcessSettings
                {
                    PolicyStartDateShift = TimeSpan.FromDays(shift)
                };
            }
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
        private readonly ILogger<AccommodationBookingSettingsService> _logger;
        
        private static readonly TimeSpan SettingsCacheLifetime = TimeSpan.FromMinutes(5);
        private readonly IRootAgencySystemSettingsService _rootAgencySystemSettingsService;
    }
}