using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.Edo.DirectApi.Services.AvailabilitySearch;

namespace HappyTravel.Edo.DirectApi.Services.Overriden
{
    public class DirectApiRedisWideAvailabilityStorage : IWideAvailabilityStorage
    {
        public DirectApiRedisWideAvailabilityStorage(IMultiProviderAvailabilityStorage multiProviderAvailabilityStorage)
        {
            _multiProviderAvailabilityStorage = multiProviderAvailabilityStorage;
        }

        
        public async Task<List<(string SupplierCode, List<AccommodationAvailabilityResult> AccommodationAvailabilities)>> GetResults(Guid searchId, List<string> suppliers)
        {
            return (await _multiProviderAvailabilityStorage.Get<List<AccommodationAvailabilityResult>>(searchId.ToString(), suppliers, true))
                .Where(t => t.Result != default)
                .ToList();
        }


        public async Task<List<WideAvailabilityResult>> GetFilteredResults(Guid searchId, AvailabilitySearchFilter filters, AccommodationBookingSettings searchSettings, List<string> suppliers, string languageCode)
        {
            var results = await GetResults(searchId, suppliers);
            var availabilities = results.SelectMany(r => r.AccommodationAvailabilities)
                .GroupBy(a => a.HtId)
                .Select(g => g.OrderBy(a => a.Created).First())
                .AsQueryable();

            if (searchSettings.AprMode == AprMode.Hide)
                availabilities = availabilities.Where(a => a.RoomContractSets.All(rcs => !rcs.IsAdvancePurchaseRate));
            
            if (searchSettings.PassedDeadlineOffersMode == PassedDeadlineOffersMode.Hide)
                availabilities = availabilities.Where(a => a.RoomContractSets.All(rcs => rcs.Deadline.Date == null || rcs.Deadline.Date >= DateTime.UtcNow));

            return availabilities
                .OrderBy(a => a.Created)
                .ThenBy(a => a.HtId)
                .ToWideAvailabilityResults(searchSettings);
        }


        public Task SaveResults(Guid searchId, string supplierCode, List<AccommodationAvailabilityResult> results) 
            => _multiProviderAvailabilityStorage.Save(searchId.ToString(), results, supplierCode);
        
        
        private readonly IMultiProviderAvailabilityStorage _multiProviderAvailabilityStorage;
    }
}