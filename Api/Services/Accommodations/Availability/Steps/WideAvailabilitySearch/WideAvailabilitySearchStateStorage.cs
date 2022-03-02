using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public class WideAvailabilitySearchStateStorage : IWideAvailabilitySearchStateStorage
    {
        public WideAvailabilitySearchStateStorage(IMultiProviderAvailabilityStorage multiProviderAvailabilityStorage)
        {
            _multiProviderAvailabilityStorage = multiProviderAvailabilityStorage;
        }
        
        
        public async Task<List<(string SupplierCode, SupplierAvailabilitySearchState States)>> GetStates(Guid searchId, List<string> suppliers) 
            => (await _multiProviderAvailabilityStorage.Get<SupplierAvailabilitySearchState>(searchId.ToString(), suppliers, false))
                .Where(t => !t.Result.Equals(default))
                .ToList();


        public Task SaveState(Guid searchId, SupplierAvailabilitySearchState state, string supplierCode) 
            => _multiProviderAvailabilityStorage.Save(searchId.ToString(), state, supplierCode);


        private readonly IMultiProviderAvailabilityStorage _multiProviderAvailabilityStorage;
    }
}