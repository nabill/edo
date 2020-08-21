using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public interface IMultiProviderAvailabilityStorage
    {
        Task<(DataProviders DataProvider, TObject Result)[]> Get<TObject>(string keyPrefix, List<DataProviders> dataProviders, bool isCachingEnabled = false);

        Task Save<TObjectType>(string keyPrefix, TObjectType @object, DataProviders dataProvider);
    }
}