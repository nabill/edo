using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public interface IWideAvailabilityStorage
    {
        Task<List<(string SupplierCode, List<AccommodationAvailabilityResult> AccommodationAvailabilities)>> GetResults(Guid searchId, AccommodationBookingSettings searchSettings);

        Task<List<AccommodationAvailabilityResult>> GetResults(string supplierCode, Guid searchId, AccommodationBookingSettings searchSettings);

        Task<List<AccommodationAvailabilityResult>> GetFilteredResults(Guid searchId, AvailabilitySearchFilter? filters, AccommodationBookingSettings searchSettings, List<string> suppliers);

        Task SaveResults(List<AccommodationAvailabilityResult> results, string requestHash);

        Task<Guid> GetSearchId(string requestHash);
    }
}