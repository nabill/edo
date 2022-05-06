using System;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Money.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace HappyTravel.Edo.Api.Infrastructure.MongoDb.Serializers
{
    public class SerializationProvider : IBsonSerializationProvider
    {
        public IBsonSerializer? GetSerializer(Type type) 
            => type switch
            {
                _ when type == typeof(Rate) => new RateSerializer(),
                _ when type == typeof(MoneyAmount) => new MoneyAmountSerializer(),
                _ when type == typeof(DailyRate) => new DailyRateSerializer(),
                // By default MongoDB serialized decimals as strings
                _ when type == typeof(decimal) => new DecimalSerializer(BsonType.Decimal128),
                _ when type == typeof(decimal?) => new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)),
                _ => null // default serializer will be used
            };
    }
}