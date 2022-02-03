using HappyTravel.Edo.Api.Infrastructure.MongoDb.Extensions;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using HappyTravel.Edo.DirectApi.Services;
using HappyTravel.Edo.DirectApi.Services.Overriden;
using HappyTravel.VaultClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HappyTravel.Edo.DirectApi.Infrastructure.Extensions
{
    internal static class ConfigureWideAvailabilityStorageExtension
    {
        public static IServiceCollection ConfigureWideAvailabilityStorage(this IServiceCollection collection, IHostEnvironment hostEnvironment, IConfiguration configuration, IVaultClient vaultClient)
        {
            // using overridden wide availability storage services because static accommodation information is not needed in direct api
            
            var useMongoDbStorage  = configuration.GetValue<bool>("WideAvailabilityStorage:UseMongoDbStorage");
            if (useMongoDbStorage)
            {
                collection.AddMongoDbStorage(hostEnvironment, configuration, vaultClient);
                collection.AddTransient<IWideAvailabilityStorage, DirectApiMongoDbWideAvailabilityStorage>();
            }
            else
            {
                collection.AddTransient<IWideAvailabilityStorage, DirectApiRedisWideAvailabilityStorage>();
            }

            return collection;
        }
    }
}