using System.Collections.Generic;
using HappyTravel.Edo.Api.Infrastructure.MongoDb.Extensions;
using HappyTravel.EdoContracts.General;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Models;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Rate = HappyTravel.Edo.Api.Models.Accommodations.Rate;

namespace HappyTravel.Edo.Api.Infrastructure.MongoDb.Serializers
{
    public class RateSerializer : SerializerBase<Rate>
    {
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Rate rate)
        {
            var writer = context.Writer;

            writer.WriteStartDocument();
            writer.Write(nameof(rate.FinalPrice), rate.FinalPrice);
            writer.Write(nameof(rate.Gross), rate.Gross);
            writer.Write(nameof(rate.Discounts), rate.Discounts);
            writer.Write(nameof(rate.Type), rate.Type);
            writer.Write(nameof(rate.Description), rate.Description);
            writer.WriteEndDocument();
        }

        public override Rate Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var reader = context.Reader;

            reader.ReadStartDocument();
            var finalPrice = reader.Read<MoneyAmount>();
            var gross = reader.Read<MoneyAmount>();
            var discounts = reader.Read<List<Discount>>();
            var type = reader.Read<PriceTypes>();
            var description = reader.Read<string>();
            reader.ReadEndDocument();

            return new Rate(finalPrice: finalPrice, 
                gross: gross, 
                discounts: discounts, 
                type: type, 
                description: description);
        }
    }
}