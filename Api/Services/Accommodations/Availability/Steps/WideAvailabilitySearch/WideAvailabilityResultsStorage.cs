using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public class WideAvailabilityResultsStorage : IWideAvailabilityResultsStorage
    {
        public WideAvailabilityResultsStorage(IAvailabilityStorage availabilityStorage)
        {
            _availabilityStorage = availabilityStorage;
        }


        public async Task<IReadOnlyCollection<(DataProviders ProviderKey, AccommodationAvailabilityResult[] AccommodationAvailabilities)>> GetResults(Guid searchId, List<DataProviders> dataProviders)
        {
            return  (await _availabilityStorage.GetProviderResults<AccommodationAvailabilityResult[]>(searchId, dataProviders, true))
                .Where(t => !t.Result.Equals(default))
                .ToList();
        }


        public Task<(DataProviders ProviderKey, ProviderAvailabilitySearchState States)[]> GetStates(Guid searchId,
            List<DataProviders> dataProviders)
        {
            return _availabilityStorage.GetProviderResults<ProviderAvailabilitySearchState>(searchId, dataProviders, false);
        }


        public Task SaveState(Guid searchId, ProviderAvailabilitySearchState state, DataProviders dataProviders)
        {
            return _availabilityStorage.SaveObject(searchId, state, dataProviders);
        }


        public Task SaveResults(Guid searchId, DataProviders dataProvider, AccommodationAvailabilityResult[] results)
        {
            return _availabilityStorage.SaveObject(searchId, results, dataProvider);
        }
        
        private readonly IAvailabilityStorage _availabilityStorage;
    }
}