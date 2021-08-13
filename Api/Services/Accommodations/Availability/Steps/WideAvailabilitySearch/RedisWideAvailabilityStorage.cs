using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.SuppliersCatalog;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public class RedisWideAvailabilityStorage : IWideAvailabilityStorage
    {
        public RedisWideAvailabilityStorage(IMultiProviderAvailabilityStorage multiProviderAvailabilityStorage)
        {
            _multiProviderAvailabilityStorage = multiProviderAvailabilityStorage;
        }


        public async Task<List<(Suppliers SupplierKey, List<AccommodationAvailabilityResult> AccommodationAvailabilities)>> GetResults(Guid searchId, List<Suppliers> suppliers)
        {
            return  (await _multiProviderAvailabilityStorage.Get<List<AccommodationAvailabilityResult>>(searchId.ToString(), suppliers, true))
                .Where(t => t.Result != default)
                .ToList();
        }


        public Task SaveResults(Guid searchId, Suppliers supplier, List<AccommodationAvailabilityResult> results)
        {
            return _multiProviderAvailabilityStorage.Save(searchId.ToString(), results, supplier);
        }
        
        private readonly IMultiProviderAvailabilityStorage _multiProviderAvailabilityStorage;
    }
}