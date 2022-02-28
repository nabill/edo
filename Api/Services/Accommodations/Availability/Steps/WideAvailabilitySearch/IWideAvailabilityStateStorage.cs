using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public interface IWideAvailabilitySearchStateStorage
    {
        Task<List<(string SupplierCode, SupplierAvailabilitySearchState States)>> GetStates(Guid searchId, List<string> suppliers);
        Task SaveState(Guid searchId, SupplierAvailabilitySearchState state, string supplierCode);
    }
}