using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public interface IAvailabilityStorage
    {
        Task<(DataProviders DataProvider, TObject Result)[]> GetProviderResults<TObject>(Guid searchId, List<DataProviders> dataProviders, bool isCachingEnabled = false);

        Task SaveObject<TObjectType>(Guid searchId, TObjectType @object, DataProviders? dataProvider = null);
    }
}