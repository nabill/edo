using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.MongoDb.Interfaces;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.SuppliersCatalog;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public class MongoDbWideAvailabilityStorage : IWideAvailabilityStorage
    {
        public MongoDbWideAvailabilityStorage(IMongoDbStorage<AccommodationAvailabilityResult> availabilityStorage)
        {
            _availabilityStorage = availabilityStorage;
        }
        
        public Task<List<(Suppliers SupplierKey, List<AccommodationAvailabilityResult> AccommodationAvailabilities)>> GetResults(Guid searchId, List<Suppliers> suppliers) 
            => throw new NotImplementedException();


        public Task SaveResults(Guid searchId, Suppliers supplier, List<AccommodationAvailabilityResult> results)
            => results.Any()
                ? _availabilityStorage.Add(results)
                : Task.CompletedTask;


        private readonly IMongoDbStorage<AccommodationAvailabilityResult> _availabilityStorage;
    }
}