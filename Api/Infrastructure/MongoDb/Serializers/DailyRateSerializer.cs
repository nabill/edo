using System;
using HappyTravel.Edo.Api.Infrastructure.MongoDb.Extensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Models;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace HappyTravel.Edo.Api.Infrastructure.MongoDb.Serializers
{
    public class DailyRateSerializer : SerializerBase<DailyRate>
    {
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DailyRate dailyRate)
        {
            var writer = context.Writer;

            writer.WriteStartDocument();
            writer.Write(nameof(dailyRate.FromDate), dailyRate.FromDate);
            writer.Write(nameof(dailyRate.ToDate), dailyRate.ToDate);
            writer.Write(nameof(dailyRate.Description), dailyRate.Description);
            writer.Write(nameof(dailyRate.Gross), dailyRate.Gross);
            writer.Write(nameof(dailyRate.FinalPrice), dailyRate.FinalPrice);
            writer.Write(nameof(dailyRate.Type), dailyRate.Type);
            writer.WriteEndDocument();
        }

        public override DailyRate Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var reader = context.Reader;

            reader.ReadStartDocument();
            var fromDate = reader.Read<DateTime>();
            var toDate = reader.Read<DateTime>();
            var description = reader.Read<string>();
            var gross = reader.Read<MoneyAmount>();
            var finalPrice = reader.Read<MoneyAmount>();
            var type = reader.Read<PriceTypes>();
            reader.ReadEndDocument();

            return new DailyRate(fromDate: fromDate,
                toDate: toDate, finalPrice: finalPrice,
                gross: gross,
                type: type,
                description: description);
        }
    }
}