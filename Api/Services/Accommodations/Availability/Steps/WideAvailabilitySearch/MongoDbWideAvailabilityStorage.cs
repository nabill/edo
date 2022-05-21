using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.MongoDb.Interfaces;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.MapperContracts.Public.Accommodations.Enums;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public class MongoDbWideAvailabilityStorage : IWideAvailabilityStorage
    {
        public MongoDbWideAvailabilityStorage(IMongoDbStorage<CachedAccommodationAvailabilityResult> availabilityStorage, IDateTimeProvider dateTimeProvider, 
            IAccommodationMapperClient mapperClient)
        {
            _availabilityStorage = availabilityStorage;
            _dateTimeProvider = dateTimeProvider;
            _mapperClient = mapperClient;
        }
        
        
        // TODO: method added for compability with 2nd and 3rd steps. Need to refactor them for using filters instead of loading whole search results
        public async Task<List<(string SupplierCode, List<AccommodationAvailabilityResult> AccommodationAvailabilities)>> GetResults(Guid searchId, AccommodationBookingSettings searchSettings)
        {
            var entities = await _availabilityStorage.Collection()
                .Where(r => r.SearchId == searchId && searchSettings.EnabledConnectors.Contains(r.SupplierCode))
                .ToListAsync();

            return entities
                .Select(r => r.Map(searchSettings))
                .GroupBy(r => r.SupplierCode)
                .Select(g => (g.Key, g.ToList()))
                .ToList();
        }


        public async Task<List<AccommodationAvailabilityResult>> GetFilteredResults(Guid searchId, AvailabilitySearchFilter? filters, AccommodationBookingSettings searchSettings, List<string> suppliers)
        {
            var rows = await _availabilityStorage.Collection()
                .Where(r => r.SearchId == searchId && suppliers.Contains(r.SupplierCode))
                .Select(r => new {r.Id, r.HtId, r.Created})
                .ToListAsync();

            var htIds = new List<string>();
            var ids = new List<ObjectId>();

            foreach (var group in rows.GroupBy(r => r.HtId))
            {
                htIds.Add(group.Key);
                ids.Add(group.OrderBy(g => g.Created).First().Id);
            }

            var query = _availabilityStorage.Collection()
                .Where(r => r.SearchId == searchId && ids.Contains(r.Id));

            if (filters is not null)
            {
                query = filters.Suppliers is not null
                    ? query.Where(r => filters.Suppliers.Contains(r.SupplierCode))
                    : query.Where(r => suppliers.Contains(r.SupplierCode));

                if (filters.MinPrice is not null)
                    query = query.Where(r => r.MinPrice >= filters.MinPrice);

                if (filters.MaxPrice is not null)
                    query = query.Where(r => r.MaxPrice <= filters.MaxPrice);

                if (filters.BoardBasisTypes is not null)
                    query = query.Where(r => r.RoomContractSets.Any(rcs => rcs.Rooms.Any(room => filters.BoardBasisTypes.Contains(room.BoardBasis))));

                if (searchSettings.AprMode == AprMode.Hide)
                    query = query.Where(r => r.RoomContractSets.Any(rcs => !rcs.IsAdvancePurchaseRate));

                if (searchSettings.PassedDeadlineOffersMode == PassedDeadlineOffersMode.Hide)
                    query = query.Where(r => r.RoomContractSets.Any(rcs => rcs.Deadline.Date == null || rcs.Deadline.Date >= _dateTimeProvider.UtcNow()));

                if (filters.Ratings is not null)
                {
                    var filteredHtIds = await GetAccommodationRatings(htIds, filters.Ratings);
                    query = query.Where(r => filteredHtIds.Contains(r.HtId));
                }
            
                if (filters.Order == "price")
                {
                    query = filters.Direction switch
                    {
                        "asc" => query.OrderBy(x => x.MinPrice),
                        "desc" => query.OrderByDescending(x => x.MinPrice),
                        _ => query
                    };
                }
                else
                {
                    query = query
                        .OrderBy(r => r.Created)
                        .ThenBy(r => r.HtId);
                }

                query = query.Skip(filters.Skip)
                    .Take(filters.Top);
            }
            
            if (searchSettings.AprMode == AprMode.Hide)
                query = query.Where(r => r.RoomContractSets.Any(rcs => !rcs.IsAdvancePurchaseRate));

            if (searchSettings.PassedDeadlineOffersMode == PassedDeadlineOffersMode.Hide)
                query = query.Where(r => r.RoomContractSets.Any(rcs => rcs.Deadline.Date == null || rcs.Deadline.Date >= DateTime.UtcNow));

            var results = await query.ToListAsync();
            
            return results.Select(a => a.Map(searchSettings)).ToList();
        }


        public Task SaveResults(List<AccommodationAvailabilityResult> results, string requestHash)
            => results.Any()
                ? _availabilityStorage.Add(results.Select(r => r.Map(requestHash)))
                : Task.CompletedTask;


        public async Task<List<AccommodationAvailabilityResult>> GetResults(string supplierCode, Guid searchId, AccommodationBookingSettings searchSettings)
        {
            var cachedResults = await _availabilityStorage.Collection()
                .Where(r => r.SupplierCode == supplierCode && r.SearchId == searchId)
                .ToListAsync();

            return cachedResults.Select(r => r.Map(searchSettings))
                .ToList();
        }


        public Task<Guid> GetSearchId(string requestHash)
            => _availabilityStorage.Collection()
                .Where(r => r.RequestHash == requestHash)
                .Select(r => r.SearchId)
                .FirstOrDefaultAsync();


        private async Task<List<string>> GetAccommodationRatings(List<string> htIds, List<AccommodationRatings> ratings) 
            => await _mapperClient.FilterHtIdsByRating(htIds, ratings);


        private readonly IMongoDbStorage<CachedAccommodationAvailabilityResult> _availabilityStorage;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAccommodationMapperClient _mapperClient;
        
    }
}