using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure.DataProviders;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Common.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Connectors
{
    public class DataProviderManager : IDataProviderManager
    {
        public DataProviderManager(IOptions<DataProviderOptions> options,
            IConnectorClient connectorClient,
            IServiceProvider serviceProvider,
            IAgentSystemSettingsService agentSystemSettingsService,
            IDoubleFlow doubleFlow)
        {
            _agentSystemSettingsService = agentSystemSettingsService;
            _doubleFlow = doubleFlow;
            _options = options.Value;
            _dataProviders = new Dictionary<DataProviders, IDataProvider>
            {
                // TODO: Add other data providers.
                {
                    DataProviders.Netstorming,
                    new DataProvider(connectorClient, _options.Netstorming, serviceProvider.GetRequiredService<ILogger<DataProvider>>())
                },
                {
                    DataProviders.Illusions,
                    new DataProvider(connectorClient, _options.Illusions, serviceProvider.GetRequiredService<ILogger<DataProvider>>())
                },
                {
                    DataProviders.Etg,
                    new DataProvider(connectorClient, _options.Etg, serviceProvider.GetRequiredService<ILogger<DataProvider>>())
                }
            };
        }


        public Task<List<DataProviders>> GetEnabled(AgentContext agent)
        {
            var key = _doubleFlow.BuildKey(nameof(DataProviderManager),
                nameof(GetEnabled),
                agent.AgentId.ToString(),
                agent.AgencyId.ToString());

            return _doubleFlow.GetOrSetAsync(key, async () =>
            {
                var agentSettings = await _agentSystemSettingsService.GetAvailabilitySearchSettings(agent);
                return agentSettings.HasValue
                    ? agentSettings.Value.EnabledProviders
                    : _options.EnabledProviders;
            }, AgentEnabledConnectorsCacheLifetime);
        }


        public IDataProvider Get(DataProviders key) => _dataProviders[key];

        private static readonly TimeSpan AgentEnabledConnectorsCacheLifetime = TimeSpan.FromMinutes(5);
        
        private readonly Dictionary<DataProviders, IDataProvider> _dataProviders;
        private readonly IAgentSystemSettingsService _agentSystemSettingsService;
        private readonly IDoubleFlow _doubleFlow;
        private readonly DataProviderOptions _options;
    }
}