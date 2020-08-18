using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public class WideAvailabilityStorage : IWideAvailabilityStorage
    {
        public WideAvailabilityStorage(IMultiProviderAvailabilityStorage multiProviderAvailabilityStorage)
        {
            _multiProviderAvailabilityStorage = multiProviderAvailabilityStorage;
        }


        public async Task<List<(DataProviders ProviderKey, List<AccommodationAvailabilityResult> AccommodationAvailabilities)>> GetResults(Guid searchId, List<DataProviders> dataProviders)
        {
            return  (await _multiProviderAvailabilityStorage.Get<List<AccommodationAvailabilityResult>>(searchId.ToString(), dataProviders, true))
                .Where(t => t.Result != default)
                .ToList();
        }


        public Task<(DataProviders ProviderKey, ProviderAvailabilitySearchState States)[]> GetStates(Guid searchId,
            List<DataProviders> dataProviders)
        {
            return _multiProviderAvailabilityStorage.Get<ProviderAvailabilitySearchState>(searchId.ToString(), dataProviders, false);
        }


        public Task SaveState(Guid searchId, ProviderAvailabilitySearchState state, DataProviders dataProvider)
        {
            return _multiProviderAvailabilityStorage.Save(searchId.ToString(), state, dataProvider);
        }


        public Task SaveResults(Guid searchId, DataProviders dataProvider, List<AccommodationAvailabilityResult> results)
        {
            return _multiProviderAvailabilityStorage.Save(searchId.ToString(), results, dataProvider);
        }
        
        private readonly IMultiProviderAvailabilityStorage _multiProviderAvailabilityStorage;
    }
}