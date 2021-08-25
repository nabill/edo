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
        public static IServiceCollection AddMongoDbStorage(this IServiceCollection services, IConfiguration configuration, VaultClient.VaultClient vaultClient)
        {
            BsonSerializer.RegisterSerializationProvider(new SerializationProvider());

            var mongodbOptions = vaultClient.Get(configuration["MongoDB:Options"]).GetAwaiter().GetResult();
            services.Configure<MongoDbOptions>(o =>
            {
                o.ConnectionString = mongodbOptions["connectionString"];
                o.DatabaseName = mongodbOptions["database"];
            });

            services.AddSingleton<IMongoDbClient, MongoDbClient>();
            services.AddSingleton<IMongoDbStorage<AccommodationAvailabilityResult>, AvailabilityStorage>();

            return services;
        }
    }
}