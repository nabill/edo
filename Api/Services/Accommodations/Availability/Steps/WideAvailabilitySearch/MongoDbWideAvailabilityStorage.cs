using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.MongoDb.Interfaces;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.MapperContracts.Public.Accommodations.Enums;
using MongoDB.Bson;
using MongoDB.Driver;

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
            var filter = GetSearchIdSupplierFilterDefinition(searchId, searchSettings.EnabledConnectors);
            var cursor = await _availabilityStorage.Collection().FindAsync(filter);
            var results = await cursor.ToListAsync();

            return results
                .Select(r => r.Map(searchSettings))
                .GroupBy(r => r.SupplierCode)
                .Select(g => (g.Key, g.ToList()))
                .ToList();
        }


        public async Task<List<AccommodationAvailabilityResult>> GetFilteredResults(
            Guid searchId, AvailabilitySearchFilter? filters, AccommodationBookingSettings searchSettings, List<string> suppliers, 
            bool needFilterNonDirectContracts = false, List<string>? directContractSuppliersCodes = null)
        {
            suppliers = filters?.Suppliers != null && filters.Suppliers.Any()
                ? filters.Suppliers
                : suppliers;
            
            var (ids, htIds) = await GetIds(searchId, suppliers);
            
            var filterBuilder = Builders<CachedAccommodationAvailabilityResult>.Filter;
            var sortBuilder = Builders<CachedAccommodationAvailabilityResult>.Sort;
            var sort = sortBuilder
                .Ascending(x => x.Created)
                .Descending(x => x.IsDirectContract)
                .Ascending(x => x.HtId);

            var filter = filterBuilder.And(new[]
            {
                filterBuilder.Eq(x => x.SearchId, searchId),
                filterBuilder.In(x => x.Id, ids)
            });
            
            if (needFilterNonDirectContracts && directContractSuppliersCodes is not null)
                filter &= filterBuilder.In(x => x.SupplierCode, directContractSuppliersCodes);

            if (filters is not null)
            {
                if (filters.MinPrice is not null)
                    filter &= filterBuilder.Where(x => x.MinPrice >= filters.MinPrice);

                if (filters.MaxPrice is not null)
                    filter &= filterBuilder.Where(r => r.MaxPrice <= filters.MaxPrice);

                if (filters.BoardBasisTypes is not null)
                    filter &= filterBuilder.Where(r => r.RoomContractSets.Any(rcs => rcs.Rooms.Any(room => filters.BoardBasisTypes.Contains(room.BoardBasis))));

                if (searchSettings.AprMode == AprMode.Hide)
                    filter &= filterBuilder.Where(r => r.RoomContractSets.Any(rcs => !rcs.IsAdvancePurchaseRate));

                if (searchSettings.PassedDeadlineOffersMode == PassedDeadlineOffersMode.Hide)
                    filter &= filterBuilder.Where(r => r.RoomContractSets.Any(rcs => rcs.Deadline.Date == null || rcs.Deadline.Date >= _dateTimeProvider.UtcNow()));

                if (filters.Ratings is not null)
                {
                    var filteredHtIds = await GetAccommodationRatings(htIds, filters.Ratings);
                    filter &= filterBuilder.Where(r => filteredHtIds.Contains(r.HtId));
                }
            
                if (filters.Order == "price")
                {
                    sort  = filters.Direction switch
                    {
                        "asc" => sortBuilder.Ascending(x => x.MinPrice),
                        "desc" => sortBuilder.Descending(x => x.MinPrice),
                        _ => sort
                    };
                }
            }
            
            if (searchSettings.AprMode == AprMode.Hide)
                filter &= filterBuilder.Where(r => r.RoomContractSets.Any(rcs => !rcs.IsAdvancePurchaseRate));

            if (searchSettings.PassedDeadlineOffersMode == PassedDeadlineOffersMode.Hide)
                filter &= filterBuilder.Where(r => r.RoomContractSets.Any(rcs => rcs.Deadline.Date == null || rcs.Deadline.Date >= DateTime.UtcNow));

            var options = new FindOptions<CachedAccommodationAvailabilityResult>
            {
                Sort = sort,
                Skip = filters?.Skip,
                Limit = filters?.Top,
                Hint = new BsonString(IndexName).AsBsonValue // forcible use search index
            };
            
            var cursor = await _availabilityStorage.Collection().FindAsync(filter, options);
            var results = await cursor.ToListAsync();
            return results.Select(a => a.Map(searchSettings)).ToList();
        }


        public Task SaveResults(List<AccommodationAvailabilityResult> results, bool isDirectContract, string requestHash)
            => results.Any()
                ? _availabilityStorage.Add(results.Select(r => r.Map(isDirectContract, requestHash)))
                : Task.CompletedTask;


        public async Task<List<AccommodationAvailabilityResult>> GetResults(string supplierCode, Guid searchId, AccommodationBookingSettings searchSettings)
        {
            var filter = GetSearchIdSupplierFilterDefinition(searchId, supplierCode);
            var options = new FindOptions<CachedAccommodationAvailabilityResult>
            {
                Hint = new BsonString(IndexName).AsBsonValue // forcible use search index
            };
            var cursor = await _availabilityStorage.Collection().FindAsync(filter, options);
            var results = await cursor.ToListAsync();

            return results.Select(r => r.Map(searchSettings))
                .ToList();
        }


        public async Task<Guid> GetSearchId(string requestHash)
        {
            var date = _dateTimeProvider.UtcNow().Subtract(_searchIdExpirationBuffer);
            var filterBuilder = Builders<CachedAccommodationAvailabilityResult>.Filter;
            var projection = Builders<CachedAccommodationAvailabilityResult>.Projection
                .Expression(x => new CachedAccommodationAvailabilityResult
                {
                    SearchId = x.SearchId
                });

            var filter = filterBuilder.And(new[]
            {
                filterBuilder.Eq(x => x.RequestHash, requestHash),
                filterBuilder.Gte(x => x.ExpiredAfter, date)
            });

            var options = new FindOptions<CachedAccommodationAvailabilityResult>
            {
                Projection = projection,
                Limit = 1
            };
            
            var cursor = await _availabilityStorage.Collection().FindAsync(filter, options);
            var results = await cursor.ToListAsync();
            
            return results
                .Select(x => x.SearchId)
                .FirstOrDefault();
        }


        public Task Clear(string supplierCode, Guid searchId)
        {
            var filter = GetSearchIdSupplierFilterDefinition(searchId, supplierCode);
            return _availabilityStorage.Collection().DeleteManyAsync(filter);
        }


        private async Task<List<string>> GetAccommodationRatings(List<string> htIds, List<AccommodationRatings> ratings) 
            => await _mapperClient.FilterHtIdsByRating(htIds, ratings);


        private static FilterDefinition<CachedAccommodationAvailabilityResult> GetSearchIdSupplierFilterDefinition(Guid searchId, IEnumerable<string> suppliers)
        {
            var filterBuilder = Builders<CachedAccommodationAvailabilityResult>.Filter;
            
            return filterBuilder.And(new[]
            {
                filterBuilder.Eq(x => x.SearchId, searchId),
                filterBuilder.In(x => x.SupplierCode, suppliers)
            });
        }
        
        
        private static FilterDefinition<CachedAccommodationAvailabilityResult> GetSearchIdSupplierFilterDefinition(Guid searchId, string supplierCode)
        {
            var filterBuilder = Builders<CachedAccommodationAvailabilityResult>.Filter;
            
            return filterBuilder.And(new[]
            {
                filterBuilder.Eq(x => x.SearchId, searchId),
                filterBuilder.Eq(x => x.SupplierCode, supplierCode)
            });
        }


        private async Task<(List<ObjectId>, List<string>)> GetIds(Guid searchId, IEnumerable<string> suppliers)
        {
            var filter = GetSearchIdSupplierFilterDefinition(searchId, suppliers);
            var projection = Builders<CachedAccommodationAvailabilityResult>.Projection
                .Expression(x => new CachedAccommodationAvailabilityResult
                {
                    Id = x.Id,
                    HtId = x.HtId,
                    Created = x.Created
                });

            var options = new FindOptions<CachedAccommodationAvailabilityResult> { Projection = projection };
            var cursor =  await _availabilityStorage.Collection().FindAsync(filter, options);
            var rows = await cursor.ToListAsync();

            var htIds = new List<string>();
            var ids = new List<ObjectId>();

            foreach (var group in rows.GroupBy(r => r.HtId))
            {
                htIds.Add(group.Key);
                ids.Add(group.OrderBy(g => g.Created).First().Id);
            }

            return new ValueTuple<List<ObjectId>, List<string>>(ids, htIds);
        }
        
        
        private readonly TimeSpan _searchIdExpirationBuffer = TimeSpan.FromMinutes(15);
        private const string IndexName = "SearchId_1_SupplierCode_1"; 


        private readonly IMongoDbStorage<CachedAccommodationAvailabilityResult> _availabilityStorage;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAccommodationMapperClient _mapperClient;
    }
}