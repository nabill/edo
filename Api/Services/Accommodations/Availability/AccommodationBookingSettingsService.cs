using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.EdoContracts.General.Enums;
using Microsoft.Extensions.Logging;
using Api.AdministratorServices;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public class AccommodationBookingSettingsService : IAccommodationBookingSettingsService
    {
        public AccommodationBookingSettingsService(IDoubleFlow doubleFlow,
            IAgentSystemSettingsService agentSystemSettingsService,
            IAgencySystemSettingsService agencySystemSettingsService,
            IRootAgencySystemSettingsService rootAgencySystemSettingsService,
            IAgentSupplierManagementService agentSupplierManagementService,
            ILogger<AccommodationBookingSettingsService> logger
            )
        {
            _doubleFlow = doubleFlow;
            _agentSystemSettingsService = agentSystemSettingsService;
            _agencySystemSettingsService = agencySystemSettingsService;
            _rootAgencySystemSettingsService = rootAgencySystemSettingsService;
            _agentSupplierManagementService = agentSupplierManagementService;
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

                return await MergeSettings(agentSettings, agencySettings, rootAgencySettings, agent.AgencyId);
            }, SettingsCacheLifetime);


            async Task<AccommodationBookingSettings> MergeSettings(Maybe<AgentAccommodationBookingSettings> agentSettings, Maybe<AgencyAccommodationBookingSettings> agencySettings,
                RootAgencyAccommodationBookingSettings rootAgencySettings, int agencyId)
            {
                var agentSettingsValue = agentSettings.HasValue
                    ? agentSettings.Value
                    : null;
                var agencySettingsValue = agencySettings.HasValue
                    ? agencySettings.Value
                    : null;

                List<string> enabledConnectors = await GetEnabledConnectors(agencyId, agent.AgentId);
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

                var rootShift = rootSettings.PolicyStartDateShift.Days;
                var agencyShift = 0;
                var agentShift = 0;

                if (agencySettings.HasValue && agencySettings.Value.CustomDeadlineShift.HasValue)
                    agencyShift = agencySettings.Value.CustomDeadlineShift.Value;

                if (agentSettings.HasValue && agentSettings.Value.CustomDeadlineShift.HasValue)
                    agentShift = agentSettings.Value.CustomDeadlineShift.Value;

                var totalShift = rootShift + agencyShift + agentShift;

                if (totalShift > 0)
                {
                    _logger.LogTotalDeadlineShiftIsPositive(agent.AgentId, agent.AgencyId, rootShift, agencyShift, agentShift);
                    totalShift = 0;
                }

                return new CancellationPolicyProcessSettings
                {
                    PolicyStartDateShift = TimeSpan.FromDays(totalShift)
                };
            }
        }


        private async Task<List<string>> GetEnabledConnectors(int agencyId, int agentId)
        {
            var (_, isFailure, agentSuppliers) = await _agentSupplierManagementService.GetMaterializedSuppliers(agencyId, agentId);
            return agentSuppliers.Where(s => s.Value).Select(s => s.Key).ToList();
        }

        private const PassedDeadlineOffersMode DefaultPassedDeadlineOffersMode = PassedDeadlineOffersMode.DisplayOnly;

        private const AprMode DefaultAprMode = AprMode.DisplayOnly;

        private readonly IDoubleFlow _doubleFlow;
        private readonly IAgentSystemSettingsService _agentSystemSettingsService;
        private readonly IAgencySystemSettingsService _agencySystemSettingsService;
        private readonly ILogger<AccommodationBookingSettingsService> _logger;
        private readonly IAgentSupplierManagementService _agentSupplierManagementService;
        private static readonly TimeSpan SettingsCacheLifetime = TimeSpan.FromMinutes(5);
        private readonly IRootAgencySystemSettingsService _rootAgencySystemSettingsService;
    }
}