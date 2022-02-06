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
        public DirectApiMongoDbWideAvailabilityStorage(IMongoDbStorage<AccommodationAvailabilityResult> availabilityStorage)
        {
            _availabilityStorage = availabilityStorage;
        }


        // TODO: method added for compability with 2nd and 3rd steps. Need to refactor them for using filters instead of loading whole search results
        public async Task<List<(int SupplierId, List<AccommodationAvailabilityResult> AccommodationAvailabilities)>> GetResults(Guid searchId, List<int> suppliers)
        {
            var entities = await _availabilityStorage.Collection()
                .Where(r => r.SearchId == searchId && suppliers.Contains(r.SupplierId))
                .ToListAsync();

            return entities
                .GroupBy(r => r.SupplierId)
                .Select(g => (g.Key, g.ToList()))
                .ToList();
        }


        public async Task<List<WideAvailabilityResult>> GetFilteredResults(Guid searchId, AvailabilitySearchFilter filters, AccommodationBookingSettings searchSettings, List<int> suppliers, string languageCode)
        {
            var rows = await _availabilityStorage.Collection()
                .Where(r => r.SearchId == searchId && suppliers.Contains(r.SupplierId))
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
                .ToWideAvailabilityResults(searchSettings);
        }


        public Task SaveResults(Guid searchId, int supplierId, List<AccommodationAvailabilityResult> results)
            => results.Any()
                ? _availabilityStorage.Add(results)
                : Task.CompletedTask;
        
        
        private readonly IMongoDbStorage<AccommodationAvailabilityResult> _availabilityStorage;
    }
}