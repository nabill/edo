using HappyTravel.Edo.Api.Infrastructure.MongoDb.Interfaces;
using HappyTravel.Edo.Api.Infrastructure.MongoDb.Options;
using HappyTravel.Edo.Api.Infrastructure.MongoDb.Serializers;
using HappyTravel.Edo.Api.Infrastructure.MongoDb.Services;
using HappyTravel.Edo.Api.Models.Accommodations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;

namespace HappyTravel.Edo.Api.Infrastructure.MongoDb.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMongoDbStorage(this IServiceCollection services, IConfiguration configuration)
        {
            BsonSerializer.RegisterSerializationProvider(new SerializationProvider());

            services.Configure<MongoDbOptions>(o =>
            {
                o.ConnectionString = configuration.GetValue<string>(configuration.GetValue<string>("MongoDB:ConnectionString"));
                o.DatabaseName = configuration.GetValue<string>(configuration.GetValue<string>("MongoDB:Database"));
            });

            services.AddSingleton<IMongoDbClient, MongoDbClient>();
            services.AddSingleton<IMongoDbStorage<AccommodationAvailabilityResult>, AvailabilityStorage>();

            return services;
        }
    }
}