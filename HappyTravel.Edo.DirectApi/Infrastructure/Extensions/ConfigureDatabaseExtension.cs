using HappyTravel.Edo.Data;
using HappyTravel.VaultClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.Edo.DirectApi.Infrastructure.Extensions
{
    internal static class ConfigureDatabaseExtension
    {
        public static IServiceCollection ConfigureDatabase(this IServiceCollection collection, IConfiguration configuration, IVaultClient vaultClient)
        {
            var dbOptions = vaultClient.Get(configuration["Database:Options"]).GetAwaiter().GetResult();
            
            return collection.AddDbContextPool<EdoContext>(o =>
            {
                var host = dbOptions["host"];
                var port = dbOptions["port"];
                var name = dbOptions["userId"];
                var password = dbOptions["password"];
                var user = dbOptions["userId"];
                
                var connectionString = configuration.GetConnectionString("Edo");
                o.UseNpgsql(string.Format(connectionString, host, port, name, user, password));
                o.EnableSensitiveDataLogging(false);
                o.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }, 16);
        }
    }
}