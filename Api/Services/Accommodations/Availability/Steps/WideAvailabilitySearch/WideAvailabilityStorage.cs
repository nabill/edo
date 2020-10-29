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


        public async Task<List<(Suppliers ProviderKey, List<AccommodationAvailabilityResult> AccommodationAvailabilities)>> GetResults(Guid searchId, List<Suppliers> dataProviders)
        {
            return  (await _multiProviderAvailabilityStorage.Get<List<AccommodationAvailabilityResult>>(searchId.ToString(), dataProviders, true))
                .Where(t => t.Result != default)
                .ToList();
        }


        public async Task<List<(Suppliers ProviderKey, ProviderAvailabilitySearchState States)>> GetStates(Guid searchId,
            List<Suppliers> dataProviders)
        {
            return (await _multiProviderAvailabilityStorage
                .Get<ProviderAvailabilitySearchState>(searchId.ToString(), dataProviders, false))
                .Where(t => !t.Result.Equals(default))
                .ToList();
        }


        public Task SaveState(Guid searchId, ProviderAvailabilitySearchState state, Suppliers supplier)
        {
            return _multiProviderAvailabilityStorage.Save(searchId.ToString(), state, supplier);
        }


        public Task SaveResults(Guid searchId, Suppliers supplier, List<AccommodationAvailabilityResult> results)
        {
            return _multiProviderAvailabilityStorage.Save(searchId.ToString(), results, supplier);
        }
        
        private readonly IMultiProviderAvailabilityStorage _multiProviderAvailabilityStorage;
    }
}