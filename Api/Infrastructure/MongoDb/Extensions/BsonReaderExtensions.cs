using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace HappyTravel.Edo.Api.Infrastructure.MongoDb.Extensions
{
    public static class BsonReaderExtensions
    {
        public static T Read<T>(this IBsonReader reader)
        {
            reader.ReadBsonType();
            reader.SkipName();
            return BsonSerializer.Deserialize<T>(reader);
        }
    }
}