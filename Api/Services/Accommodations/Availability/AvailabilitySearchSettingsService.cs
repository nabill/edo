using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Common.Enums;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public class AvailabilitySearchSettingsService : IAvailabilitySearchSettingsService
    {
        public AvailabilitySearchSettingsService(IDoubleFlow doubleFlow,
            IAgentSystemSettingsService agentSystemSettingsService,
            IAgencySystemSettingsService agencySystemSettingsService,
            IOptions<DataProviderOptions> dataProviderOptions)
        {
            _doubleFlow = doubleFlow;
            _agentSystemSettingsService = agentSystemSettingsService;
            _dataProviderOptions = dataProviderOptions.Value;
            _agencySystemSettingsService = agencySystemSettingsService;
        }
        
        public async Task<AvailabilitySearchSettings> Get(AgentContext agent)
        {
            var enabledConnectors = await GetEnabledConnectors(agent);
            var (_, _, aprMode, _) = await _agencySystemSettingsService.GetAdvancedPurchaseRatesSettings(agent.AgencyId);
            return new AvailabilitySearchSettings(enabledConnectors, aprMode);
        }
        
        
        private Task<List<DataProviders>> GetEnabledConnectors(AgentContext agent)
        {
            var key = _doubleFlow.BuildKey(nameof(DataProviderManager),
                nameof(GetEnabledConnectors),
                agent.AgentId.ToString(),
                agent.AgencyId.ToString());

            return _doubleFlow.GetOrSetAsync(key, async () =>
            {
                var agentSettings = await _agentSystemSettingsService.GetAvailabilitySearchSettings(agent);
                return agentSettings.HasValue
                    ? agentSettings.Value.EnabledProviders
                    : _dataProviderOptions.EnabledProviders;
            }, AgentEnabledConnectorsCacheLifetime);
        }
        
        private readonly IDoubleFlow _doubleFlow;
        private readonly IAgentSystemSettingsService _agentSystemSettingsService;
        private readonly IAgencySystemSettingsService _agencySystemSettingsService;
        private readonly DataProviderOptions _dataProviderOptions;
        
        private static readonly TimeSpan AgentEnabledConnectorsCacheLifetime = TimeSpan.FromMinutes(5);
    }
}