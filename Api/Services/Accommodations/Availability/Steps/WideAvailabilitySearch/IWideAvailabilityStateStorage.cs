using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public interface IWideAvailabilitySearchStateStorage
    {
        Task<List<(int SupplierId, SupplierAvailabilitySearchState States)>> GetStates(Guid searchId, List<int> suppliers);
        Task SaveState(Guid searchId, SupplierAvailabilitySearchState state, int supplierId);
    }
}