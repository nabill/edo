using HappyTravel.Edo.Api.Infrastructure.MongoDb.Interfaces;
using HappyTravel.Edo.Api.Infrastructure.MongoDb.Options;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace HappyTravel.Edo.Api.Infrastructure.MongoDb.Services
{
    public class MongoDbClient : IMongoDbClient
    {
        public MongoDbClient(IOptions<MongoDbOptions> options)
        {
            _options = options.Value;
            _client = new MongoClient(_options.ConnectionString);
        }


        public IMongoDatabase GetDatabase() 
            => _client.GetDatabase(_options.DatabaseName);


        private readonly MongoDbOptions _options;
        private readonly MongoClient _client;
    }
}