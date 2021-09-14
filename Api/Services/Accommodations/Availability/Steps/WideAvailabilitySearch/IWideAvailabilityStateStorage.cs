using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.SuppliersCatalog;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public interface IWideAvailabilitySearchStateStorage
    {
        Task<List<(Suppliers SupplierKey, SupplierAvailabilitySearchState States)>> GetStates(Guid searchId, List<Suppliers> suppliers);
        Task SaveState(Guid searchId, SupplierAvailabilitySearchState state, Suppliers supplier);
    }
}