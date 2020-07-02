using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HappyTravel.Edo.Api.Infrastructure.DataProviders;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Common.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Connectors
{
    public class DataProviderFactory : IDataProviderFactory
    {
        public DataProviderFactory(IOptions<DataProviderOptions> options,
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


        public IReadOnlyCollection<(DataProviders, IDataProvider)> GetAll()
        {
            var dataProvidersList = _dataProviders
                .Where(dp => _options.EnabledProviders.Contains(dp.Key))
                .Select(dp => (dp.Key, dp.Value))
                .ToList();

            return new ReadOnlyCollection<(DataProviders, IDataProvider)>(dataProvidersList);
        }


        public IReadOnlyCollection<(DataProviders, IDataProvider)> Get(IEnumerable<DataProviders> keys)
        {
            var dataProvidersList = (from key in keys
                join provider in _dataProviders
                    on key equals provider.Key
                where _options.EnabledProviders.Contains(key)
                select (provider.Key, provider.Value)).ToList();

            return new ReadOnlyCollection<(DataProviders, IDataProvider)>(dataProvidersList);
        }


        public IDataProvider Get(DataProviders key) => _dataProviders[key];

        private readonly Dictionary<DataProviders, IDataProvider> _dataProviders;
        private readonly DataProviderOptions _options;
    }
}