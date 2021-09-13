using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.SuppliersCatalog;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public class WideAvailabilitySearchStateStorage : IWideAvailabilitySearchStateStorage
    {
        public WideAvailabilitySearchStateStorage(IMultiProviderAvailabilityStorage multiProviderAvailabilityStorage)
        {
            _multiProviderAvailabilityStorage = multiProviderAvailabilityStorage;
        }
        
        
        public async Task<List<(Suppliers SupplierKey, SupplierAvailabilitySearchState States)>> GetStates(Guid searchId, List<Suppliers> suppliers) 
            => (await _multiProviderAvailabilityStorage.Get<SupplierAvailabilitySearchState>(searchId.ToString(), suppliers, false))
                .Where(t => !t.Result.Equals(default))
                .ToList();


        public Task SaveState(Guid searchId, SupplierAvailabilitySearchState state, Suppliers supplier) 
            => _multiProviderAvailabilityStorage.Save(searchId.ToString(), state, supplier);


        private readonly IMultiProviderAvailabilityStorage _multiProviderAvailabilityStorage;
    }
}