using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace HappyTravel.Edo.Api.Infrastructure.MongoDb.Extensions
{
    public static class BsonWriterExtensions
    {
        public static void Write<T>(this IBsonWriter writer, string key, T value)
        {
            writer.WriteName(key);
            BsonSerializer.Serialize(writer, value);
        }
    }
}