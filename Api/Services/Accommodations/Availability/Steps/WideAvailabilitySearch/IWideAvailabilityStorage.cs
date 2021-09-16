using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.SuppliersCatalog;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public interface IWideAvailabilityStorage
    {
        Task<List<(Suppliers SupplierKey, List<AccommodationAvailabilityResult> AccommodationAvailabilities)>> GetResults(Guid searchId, List<Suppliers> suppliers);

        Task<List<WideAvailabilityResult>> GetFilteredResults(Guid searchId, AvailabilitySearchFilter filters, AccommodationBookingSettings searchSettings, List<Suppliers> suppliers, string languageCode);

        Task SaveResults(Guid searchId, Suppliers supplier, List<AccommodationAvailabilityResult> results);
    }
}