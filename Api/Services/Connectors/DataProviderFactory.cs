using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HappyTravel.Edo.Api.Infrastructure.DataProviders;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Services.Locations;
using HappyTravel.Edo.Common.Enums;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Connectors
{
    public class DataProviderFactory : IDataProviderFactory
    {
        public DataProviderFactory(IOptions<DataProviderOptions> options, IDataProviderClient dataProviderClient, ILocationService locationService)
        {
            _options = options.Value;
            _dataProviders = new Dictionary<DataProviders, IDataProvider>
            {
                // TODO: Add other data providers.
                {DataProviders.Netstorming, new DataProvider(dataProviderClient, locationService, _options.Netstorming)}
            };
        }

        
        public IReadOnlyCollection<(DataProviders, IDataProvider)> GetAll()
        {
            var dataProvidersList = _dataProviders
                .Where(dp=> _options.EnabledProviders.Contains(dp.Key))
                .Select(dp => (dp.Key, dp.Value))
                .ToList();
            
            return new ReadOnlyCollection<(DataProviders, IDataProvider)>(dataProvidersList);
        }


        private readonly Dictionary<DataProviders, IDataProvider> _dataProviders;
        private readonly DataProviderOptions _options;
    }
}