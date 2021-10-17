﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.MongoDb.Interfaces;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.SuppliersCatalog;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace HappyTravel.Edo.DirectApi.Services
{
    internal class DirectApiMongoDbWideAvailabilityStorage : IWideAvailabilityStorage
    {
        public DirectApiMongoDbWideAvailabilityStorage(IMongoDbStorage<AccommodationAvailabilityResult> availabilityStorage)
        {
            _availabilityStorage = availabilityStorage;
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
            var rows = await _availabilityStorage.Collection()
                .Where(r => r.SearchId == searchId && suppliers.Contains(r.Supplier))
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

            var results = await query
                .OrderBy(r => r.Created)
                .ThenBy(r => r.HtId)
                .ToListAsync();
            
            return results
                .Select(a =>
                {
                    var roomContractSets = a.RoomContractSets
                        .Select(r => r.ApplySearchSettings(searchSettings.IsSupplierVisible, searchSettings.IsDirectContractFlagVisible))
                        .ToList();

                    if (searchSettings.AprMode == AprMode.Hide)
                        roomContractSets = roomContractSets.Where(r => !r.IsAdvancePurchaseRate).ToList();

                    if (searchSettings.PassedDeadlineOffersMode == PassedDeadlineOffersMode.Hide)
                        roomContractSets = roomContractSets.Where(r => r.Deadline.Date == null || r.Deadline.Date >= DateTime.UtcNow).ToList();

                    return new WideAvailabilityResult(default,
                        roomContractSets,
                        a.MinPrice,
                        a.MaxPrice,
                        a.CheckInDate,
                        a.CheckOutDate,
                        searchSettings.IsSupplierVisible
                            ? a.Supplier
                            : null,
                        a.HtId);
                }).ToList();
        }


        public Task SaveResults(Guid searchId, Suppliers supplier, List<AccommodationAvailabilityResult> results)
            => results.Any()
                ? _availabilityStorage.Add(results)
                : Task.CompletedTask;
        
        
        private readonly IMongoDbStorage<AccommodationAvailabilityResult> _availabilityStorage;
    }
}