using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.MongoDb.Interfaces;
using HappyTravel.Edo.Api.Models.Accommodations;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace HappyTravel.Edo.Api.Infrastructure.MongoDb.Services
{
    public class AvailabilityStorage : IMongoDbStorage<CachedAccommodationAvailabilityResult>
    {
        public AvailabilityStorage(IMongoDbClient client)
        {
            _collection = GetOrCreateCollection(client).GetAwaiter().GetResult();
        }


        public Task Add(IEnumerable<CachedAccommodationAvailabilityResult> records) 
            => _collection.InsertManyAsync(records);


        public Task Add(CachedAccommodationAvailabilityResult record) 
            => _collection.InsertOneAsync(record);


        public IMongoCollection<CachedAccommodationAvailabilityResult> Collection() 
            => _collection;


        private static async Task<IMongoCollection<CachedAccommodationAvailabilityResult>> GetOrCreateCollection(IMongoDbClient client)
        {
            var collection = client.GetDatabase().GetCollection<CachedAccommodationAvailabilityResult>(nameof(CachedAccommodationAvailabilityResult));

            var searchIndexDefinition = Builders<CachedAccommodationAvailabilityResult>.IndexKeys.Combine(
                Builders<CachedAccommodationAvailabilityResult>.IndexKeys.Ascending(f => f.SearchId),
                Builders<CachedAccommodationAvailabilityResult>.IndexKeys.Ascending(f => f.SupplierCode));

            var ttlIndexDefinition = Builders<CachedAccommodationAvailabilityResult>.IndexKeys.Ascending(f => f.ExpiredAfter);
            var ttlIndexOptions = new CreateIndexOptions {ExpireAfter = TimeSpan.FromSeconds(1)};
            
            var searchRequestIndexDefinition = Builders<CachedAccommodationAvailabilityResult>.IndexKeys.Ascending(f => f.RequestHash);

            await collection.Indexes.CreateManyAsync(new []
            {
                new CreateIndexModel<CachedAccommodationAvailabilityResult>(searchIndexDefinition),
                new CreateIndexModel<CachedAccommodationAvailabilityResult>(ttlIndexDefinition, ttlIndexOptions),
                new CreateIndexModel<CachedAccommodationAvailabilityResult>(searchRequestIndexDefinition)
            });

            return collection;
        }

        
        private readonly IMongoCollection<CachedAccommodationAvailabilityResult> _collection;
    }
}