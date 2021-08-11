using MongoDB.Driver;

namespace HappyTravel.Edo.Api.Infrastructure.MongoDb.Interfaces
{
    public interface IMongoDbClient
    {
        public IMongoDatabase GetDatabase();
    }
}