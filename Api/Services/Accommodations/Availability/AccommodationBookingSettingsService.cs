using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.Edo.Data.Agents;
using Microsoft.Extensions.Logging;
using Api.AdministratorServices;
using System.Text.Json;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public class AccommodationBookingSettingsService : IAccommodationBookingSettingsService
    {
        public AccommodationBookingSettingsService(IDoubleFlow doubleFlow,
            IAgentSystemSettingsService agentSystemSettingsService,
            IAgencySystemSettingsService agencySystemSettingsService,
            IRootAgencySystemSettingsService rootAgencySystemSettingsService,
            IAgentSupplierManagementService agentSupplierManagementService,
            ILogger<AccommodationBookingSettingsService> logger,
            IAgentContextService agentContextService)
        {
            _doubleFlow = doubleFlow;
            _agentSystemSettingsService = agentSystemSettingsService;
            _agencySystemSettingsService = agencySystemSettingsService;
            _rootAgencySystemSettingsService = rootAgencySystemSettingsService;
            _agentSupplierManagementService = agentSupplierManagementService;
            _logger = logger;
            _agentContextService = agentContextService;
        }


        public async Task<AccommodationBookingSettings> Get()
        {
            var agent = await _agentContextService.GetAgent();
            var key = _doubleFlow.BuildKey(nameof(AccommodationBookingSettingsService),
                nameof(Get),
                agent.AgentId.ToString(),
                agent.AgencyId.ToString());

            return await _doubleFlow.GetOrSetAsync(key, async () =>
            {
                List<string> enabledConnectors = await GetEnabledConnectors(agent.AgencyId, agent.AgentId);

                var agentSettings = await _agentSystemSettingsService.GetAccommodationBookingSettings(agent);
                var agencySettings = await _agencySystemSettingsService
                    .GetAccommodationBookingSettings(agent.AgencyId);

                var rootAgencySettings = await _rootAgencySystemSettingsService.GetAccommodationBookingSettings(agent.AgencyId);

                var cancellationPolicyProcessSettings =
                        MergeCancellationPolicyProcessSettings(rootAgencySettings.CancellationPolicyProcessSettings, agencySettings, agentSettings);

                return MergeSettings(agentSettings.Value, enabledConnectors, cancellationPolicyProcessSettings);
            }, SettingsCacheLifetime);


            AccommodationBookingSettings MergeSettings(AgentAccommodationBookingSettings agentSettings,
                List<string> enabledConnectors, CancellationPolicyProcessSettings cancellationPolicyProcessSettings)
                => new AccommodationBookingSettings(enabledConnectors: enabledConnectors,
                        aprMode: agentSettings.AprMode!.Value,
                        passedDeadlineOffersMode: agentSettings.PassedDeadlineOffersMode!.Value,
                        isSupplierVisible: agentSettings.IsSupplierVisible,
                        cancellationPolicyProcessSettings: cancellationPolicyProcessSettings,
                        isDirectContractFlagVisible: agentSettings.IsDirectContractFlagVisible,
                        additionalSearchFilters: agentSettings.AdditionalSearchFilters);


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
            if (isFailure)
                return new List<string>();
            return agentSuppliers.Where(s => s.Value).Select(s => s.Key).ToList();
        }

        private const PassedDeadlineOffersMode DefaultPassedDeadlineOffersMode = PassedDeadlineOffersMode.DisplayOnly;

        private const AprMode DefaultAprMode = AprMode.DisplayOnly;

        private readonly IDoubleFlow _doubleFlow;
        private readonly IAgentSystemSettingsService _agentSystemSettingsService;
        private readonly IAgencySystemSettingsService _agencySystemSettingsService;
        private readonly ILogger<AccommodationBookingSettingsService> _logger;
        private readonly IAgentSupplierManagementService _agentSupplierManagementService;
        private static readonly TimeSpan SettingsCacheLifetime = TimeSpan.FromMinutes(1);
        private readonly IRootAgencySystemSettingsService _rootAgencySystemSettingsService;
        private readonly IAgentContextService _agentContextService;
    }
}