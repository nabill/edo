using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.MongoDb.Interfaces;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.Edo.DirectApi.Services.AvailabilitySearch;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace HappyTravel.Edo.DirectApi.Services.Overriden
{
    public class DirectApiMongoDbWideAvailabilityStorage : IWideAvailabilityStorage
    {
        public DirectApiMongoDbWideAvailabilityStorage(IMongoDbStorage<CachedAccommodationAvailabilityResult> availabilityStorage)
        {
            _availabilityStorage = availabilityStorage;
        }

        
        public async Task<List<(string SupplierCode, List<AccommodationAvailabilityResult> AccommodationAvailabilities)>> GetResults(Guid searchId, List<string> suppliers)
        {
            var entities = await _availabilityStorage.Collection()
                .Where(r => r.SearchId == searchId && suppliers.Contains(r.SupplierCode))
                .ToListAsync();

            return entities
                .Select(r => r.Map())
                .GroupBy(r => r.SupplierCode)
                .Select(g => (g.Key, g.ToList()))
                .ToList();
        }


        public async Task<List<WideAvailabilityResult>> GetFilteredResults(Guid searchId, AvailabilitySearchFilter filters, AccommodationBookingSettings searchSettings, List<string> suppliers, string languageCode)
        {
            var rows = await _availabilityStorage.Collection()
                .Where(r => r.SearchId == searchId && suppliers.Contains(r.SupplierCode))
                .Select(r => new {r.Id, r.HtId, r.Created})
                .ToListAsync();
            
            var ids = rows.GroupBy(r => r.HtId)
                .Select(group => group.OrderBy(g => g.Created).First().Id)
                .ToList();

            var query = _availabilityStorage.Collection()
                .Where(r => r.SearchId == searchId && ids.Contains(r.Id));
            
            if (searchSettings.AprMode == AprMode.Hide)
                query = query.Where(r => r.RoomContractSets.Any(rcs => !rcs.IsAdvancePurchaseRate));

            if (searchSettings.PassedDeadlineOffersMode == PassedDeadlineOffersMode.Hide)
                query = query.Where(r => r.RoomContractSets.Any(rcs => rcs.Deadline.Date == null || rcs.Deadline.Date >= DateTime.UtcNow));

            return (await query
                    .OrderBy(r => r.Created)
                    .ThenBy(r => r.HtId)
                    .ToListAsync())
                .Select(r => r.Map())
                .ToWideAvailabilityResults(searchSettings);
        }


        public Task SaveResults(Guid searchId, string supplierCode, List<AccommodationAvailabilityResult> results, string requestHash)
            => results.Any()
                ? _availabilityStorage.Add(results.Select(r => r.Map(requestHash)))
                : Task.CompletedTask;
        
        
        private readonly IMongoDbStorage<CachedAccommodationAvailabilityResult> _availabilityStorage;
    }
}