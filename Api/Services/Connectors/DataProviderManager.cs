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
            IServiceProvider serviceProvider)
        {
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

        public IDataProvider Get(DataProviders key) => _dataProviders[key];
        
        private readonly Dictionary<DataProviders, IDataProvider> _dataProviders;
        private readonly DataProviderOptions _options;
    }
}