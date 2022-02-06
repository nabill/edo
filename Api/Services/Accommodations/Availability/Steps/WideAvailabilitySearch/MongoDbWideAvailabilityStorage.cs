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
using HappyTravel.SupplierOptionsProvider;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public class MongoDbWideAvailabilityStorage : IWideAvailabilityStorage
    {
        public MongoDbWideAvailabilityStorage(IMongoDbStorage<AccommodationAvailabilityResult> availabilityStorage, IDateTimeProvider dateTimeProvider, 
            IAccommodationMapperClient mapperClient, IWideAvailabilityAccommodationsStorage accommodationsStorage, ISupplierOptionsStorage supplierOptionsStorage)
        {
            _availabilityStorage = availabilityStorage;
            _dateTimeProvider = dateTimeProvider;
            _mapperClient = mapperClient;
            _accommodationsStorage = accommodationsStorage;
            _supplierOptionsStorage = supplierOptionsStorage;
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

            var htIds = new List<string>();
            var ids = new List<ObjectId>();

            foreach (var group in rows.GroupBy(r => r.HtId))
            {
                htIds.Add(group.Key);
                ids.Add(group.OrderBy(g => g.Created).First().Id);
            }
            
            await _accommodationsStorage.EnsureAccommodationsCached(htIds, languageCode);
            
            var query = _availabilityStorage.Collection()
                .Where(r => r.SearchId == searchId && ids.Contains(r.Id));

            query = filters.Suppliers is not null
                ? query.Where(r => filters.Suppliers.Contains(r.SupplierId))
                : query.Where(r => suppliers.Contains(r.SupplierId));

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
                    "desc" => query.OrderByDescending(x => x.MinPrice)
                };
            }
            else
            {
                query = query
                    .OrderBy(r => r.Created)
                    .ThenBy(r => r.HtId);
            }

            var results = await query
                .Skip(filters.Skip)
                .Take(filters.Top)
                .ToListAsync();
            
            return results
                .Select(a =>
                {
                    var accommodation = _accommodationsStorage.GetAccommodation(a.HtId, languageCode);
                    var roomContractSets = a.RoomContractSets
                        .Select(r => r.ApplySearchSettings(searchSettings.IsSupplierVisible, searchSettings.IsDirectContractFlagVisible))
                        .ToList();

                    if (searchSettings.AprMode == AprMode.Hide)
                        roomContractSets = roomContractSets.Where(r => !r.IsAdvancePurchaseRate).ToList();

                    if (searchSettings.PassedDeadlineOffersMode == PassedDeadlineOffersMode.Hide)
                        roomContractSets = roomContractSets.Where(r => r.Deadline.Date == null || r.Deadline.Date >= _dateTimeProvider.UtcNow()).ToList();

                    return new WideAvailabilityResult(accommodation: accommodation,
                        roomContractSets: roomContractSets,
                        minPrice: roomContractSets.Min(r => r.Rate.FinalPrice.Amount),
                        maxPrice: roomContractSets.Max(r => r.Rate.FinalPrice.Amount),
                        checkInDate: a.CheckInDate,
                        checkOutDate: a.CheckOutDate,
                        supplierId: searchSettings.IsSupplierVisible
                            ? a.SupplierId
                            : null,
                        htId: a.HtId);
                }).ToList();
        }


        public Task SaveResults(Guid searchId, int supplierId, List<AccommodationAvailabilityResult> results)
            => results.Any()
                ? _availabilityStorage.Add(results)
                : Task.CompletedTask;
        
        
        private async Task<List<string>> GetAccommodationRatings(List<string> htIds, List<AccommodationRatings> ratings) 
            => await _mapperClient.FilterHtIdsByRating(htIds, ratings);


        private string GetSupplierName(int supplierId) 
            => _supplierOptionsStorage.GetById(supplierId).Name;


        private readonly IMongoDbStorage<AccommodationAvailabilityResult> _availabilityStorage;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAccommodationMapperClient _mapperClient;
        private readonly ISupplierOptionsStorage _supplierOptionsStorage;
        private readonly IWideAvailabilityAccommodationsStorage _accommodationsStorage;
    }
}