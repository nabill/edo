using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.MongoDb.Interfaces;
using HappyTravel.Edo.Api.Models.Accommodations;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace HappyTravel.Edo.Api.Infrastructure.MongoDb.Services
{
    public class AvailabilityStorage : IMongoDbStorage<AccommodationAvailabilityResult>
    {
        public AvailabilityStorage(IMongoDbClient client)
        {
            _collection = GetOrCreateCollection(client).GetAwaiter().GetResult();
        }


        public Task Add(IEnumerable<AccommodationAvailabilityResult> records) 
            => _collection.InsertManyAsync(records);


        public Task Add(AccommodationAvailabilityResult record) 
            => _collection.InsertOneAsync(record);


        public IMongoQueryable<AccommodationAvailabilityResult> Collection() 
            => _collection.AsQueryable();


        private static async Task<IMongoCollection<AccommodationAvailabilityResult>> GetOrCreateCollection(IMongoDbClient client)
        {
            var collection = client.GetDatabase().GetCollection<AccommodationAvailabilityResult>(nameof(AccommodationAvailabilityResult));

            var searchIndexDefinition = Builders<AccommodationAvailabilityResult>.IndexKeys.Combine(
                Builders<AccommodationAvailabilityResult>.IndexKeys.Ascending(f => f.SearchId),
                Builders<AccommodationAvailabilityResult>.IndexKeys.Ascending(f => f.Supplier));

            var ttlIndexDefinition = Builders<AccommodationAvailabilityResult>.IndexKeys.Ascending(f => f.Created);
            var ttlIndexOptions = new CreateIndexOptions {ExpireAfter = TimeSpan.FromMinutes(45)};

            await collection.Indexes.CreateManyAsync(new []
            {
                new CreateIndexModel<AccommodationAvailabilityResult>(searchIndexDefinition),
                new CreateIndexModel<AccommodationAvailabilityResult>(ttlIndexDefinition, ttlIndexOptions)
            });

            return collection;
        }


        private readonly IMongoCollection<AccommodationAvailabilityResult> _collection;
    }
}