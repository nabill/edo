using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public interface IWideAvailabilityStorage
    {
        Task<List<(string SupplierCode, List<AccommodationAvailabilityResult> AccommodationAvailabilities)>> GetResults(Guid searchId, string htId, AccommodationBookingSettings searchSettings);

        Task<List<AccommodationAvailabilityResult>> GetResults(string supplierCode, Guid searchId, AccommodationBookingSettings searchSettings);

        Task<List<AccommodationAvailabilityResult>> GetFilteredResults(
            Guid searchId, AvailabilitySearchFilter? filters, AccommodationBookingSettings searchSettings, List<string> suppliers,
            bool needFilterNonDirectContracts = false, List<string>? directContractSuppliersCodes = null);

        Task SaveResults(List<AccommodationAvailabilityResult> results, bool isDirectContract, string requestHash);

        Task<Guid> GetSearchId(string requestHash);

        Task Clear(string supplierCode, Guid searchId);
        Task ClearByHtId(string supplierCode, Guid searchId, string htId);
    }
}