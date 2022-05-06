using System.Linq;
using HappyTravel.Edo.Api.Infrastructure.Environments;
using HappyTravel.Edo.Api.Infrastructure.MongoDb.Interfaces;
using HappyTravel.Edo.Api.Infrastructure.MongoDb.Options;
using HappyTravel.Edo.Api.Infrastructure.MongoDb.Serializers;
using HappyTravel.Edo.Api.Infrastructure.MongoDb.Services;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.VaultClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson.Serialization;

namespace HappyTravel.Edo.Api.Infrastructure.MongoDb.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMongoDbStorage(this IServiceCollection services, IHostEnvironment environment, IConfiguration configuration, IVaultClient vaultClient)
        {
            BsonSerializer.RegisterSerializationProvider(new SerializationProvider());

            if (environment.IsLocal())
            {
                services.Configure<MongoDbOptions>(configuration.GetSection("MongoDB:Options"));
            }
            else
            {
                var mongodbOptions = vaultClient.Get(configuration["MongoDB:Options"]).GetAwaiter().GetResult();
                services.Configure<MongoDbOptions>(o =>
                {
                    o.ConnectionString = mongodbOptions["connectionString"];
                    o.DatabaseName = mongodbOptions["database"];
                });
            }

            services.AddSingleton<IMongoDbClient, MongoDbClient>();
            services.AddSingleton<IMongoDbStorage<CachedAccommodationAvailabilityResult>, AvailabilityStorage>();

            return services;
        }
    }
}