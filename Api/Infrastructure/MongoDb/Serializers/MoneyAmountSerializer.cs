using HappyTravel.Edo.Api.Infrastructure.MongoDb.Extensions;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace HappyTravel.Edo.Api.Infrastructure.MongoDb.Serializers
{
    public class MoneyAmountSerializer : SerializerBase<MoneyAmount>
    {
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, MoneyAmount moneyAmount)
        {
            var writer = context.Writer;

            writer.WriteStartDocument();
            writer.Write(nameof(moneyAmount.Amount), moneyAmount.Amount);
            writer.Write(nameof(moneyAmount.Currency), moneyAmount.Currency);
            writer.WriteEndDocument();
        }

        public override MoneyAmount Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var reader = context.Reader;

            reader.ReadStartDocument();
            var amount = reader.Read<decimal>();
            var currency = reader.Read<Currencies>();
            reader.ReadEndDocument();

            return new MoneyAmount(amount: amount, currency: currency);
        }
    }
}