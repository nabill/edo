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
using HappyTravel.SuppliersCatalog;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public class MongoDbWideAvailabilityStorage : IWideAvailabilityStorage
    {
        public MongoDbWideAvailabilityStorage(IMongoDbStorage<AccommodationAvailabilityResult> availabilityStorage, IDateTimeProvider dateTimeProvider, 
            IAccommodationMapperClient mapperClient, IWideAvailabilityAccommodationsStorage accommodationsStorage)
        {
            _availabilityStorage = availabilityStorage;
            _dateTimeProvider = dateTimeProvider;
            _mapperClient = mapperClient;
            _accommodationsStorage = accommodationsStorage;
        }
        
        
        // TODO: method added for compability with 2nd and 3rd steps. Need to refactor them for using filters instead of loading whole search results
        public async Task<List<(Suppliers SupplierKey, List<AccommodationAvailabilityResult> AccommodationAvailabilities)>> GetResults(Guid searchId, List<Suppliers> suppliers)
        {
            var entities = await _availabilityStorage.Collection()
                .Where(r => r.SearchId == searchId && suppliers.Contains(r.Supplier))
                .ToListAsync();

            return entities
                .GroupBy(r => r.Supplier)
                .Select(g => (g.Key, g.ToList()))
                .ToList();
        }


        public async Task<List<WideAvailabilityResult>> GetFilteredResults(Guid searchId, AvailabilitySearchFilter filters, AccommodationBookingSettings searchSettings, List<Suppliers> suppliers, string languageCode)
        {
            var htIds = await _availabilityStorage.Collection()
                .Where(r => r.SearchId == searchId && suppliers.Contains(r.Supplier))
                .Select(r => r.HtId)
                .Distinct()
                .ToListAsync();
            
            await _accommodationsStorage.EnsureAccommodationsCached(htIds, languageCode);
            
            var query = _availabilityStorage.Collection()
                .Where(r => r.SearchId == searchId);

            query = filters.Suppliers is not null
                ? query.Where(r => filters.Suppliers.Contains(r.Supplier))
                : query.Where(r => suppliers.Contains(r.Supplier));

            if (filters.MinPrice is not null)
                query = query.Where(r => r.MinPrice >= filters.MinPrice);

            if (filters.MaxPrice is not null)
                query = query.Where(r => r.MaxPrice <= filters.MaxPrice);

            if (filters.BoardBasisTypes is not null)
                query = query.Where(r => r.RoomContractSets.Any(rcs => rcs.Rooms.Any(room => filters.BoardBasisTypes.Contains(room.BoardBasis))));

            if (searchSettings.AprMode == AprMode.Hide)
                query = query.Where(r => r.RoomContractSets.All(rcs => !rcs.IsAdvancePurchaseRate));

            if (searchSettings.PassedDeadlineOffersMode == PassedDeadlineOffersMode.Hide)
                query = query.Where(r => r.RoomContractSets.All(rcs => rcs.Deadline.Date == null || rcs.Deadline.Date >= _dateTimeProvider.UtcNow()));

            if (filters.Ratings is not null)
            {
                var filteredHtIds = await GetAccommodationRatings(htIds, filters.Ratings);
                query = query.Where(r => filteredHtIds.Contains(r.HtId));
            }

            query = query
                .OrderBy(r => r.Created)
                // TODO: remove duplicates
                //.GroupBy(r => r.HtId)
                //.Select(g => g.First())
                .Skip(filters.Skip)
                .Take(filters.Top);

            var results = await query.ToListAsync();
            return results.Select(a =>
            {
                var accommodation = _accommodationsStorage.GetAccommodation(a.HtId, languageCode);

                return new WideAvailabilityResult(accommodation,
                    a.RoomContractSets,
                    a.MinPrice,
                    a.MaxPrice,
                    a.CheckInDate,
                    a.CheckOutDate,
                    searchSettings.IsSupplierVisible
                        ? a.Supplier
                        : (Suppliers?)null,
                    a.HtId);
            }).ToList();
        }


        public Task SaveResults(Guid searchId, Suppliers supplier, List<AccommodationAvailabilityResult> results)
            => results.Any()
                ? _availabilityStorage.Add(results)
                : Task.CompletedTask;
        
        
        private async Task<List<string>> GetAccommodationRatings(List<string> htIds, List<AccommodationRatings> ratings) 
            => await _mapperClient.FilterHtIdsByRating(htIds, ratings);


        private readonly IMongoDbStorage<AccommodationAvailabilityResult> _availabilityStorage;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAccommodationMapperClient _mapperClient;
        private readonly IWideAvailabilityAccommodationsStorage _accommodationsStorage;
    }
}