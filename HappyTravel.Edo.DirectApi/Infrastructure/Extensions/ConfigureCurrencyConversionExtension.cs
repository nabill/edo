using System;
using HappyTravel.Edo.Api.Infrastructure.Environments;
using HappyTravel.Edo.Api.Services.CurrencyConversion;
using HappyTravel.VaultClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HappyTravel.Edo.DirectApi.Infrastructure.Extensions
{
    internal static class ConfigureCurrencyConversionExtension
    {
        public static IServiceCollection ConfigureCurrencyConversion(this IServiceCollection collection, IConfiguration configuration, IHostEnvironment environment, IVaultClient vaultClient)
        {
            var currencyConverterOptions = vaultClient.Get(configuration["CurrencyConverter:Options"]).GetAwaiter().GetResult();
            
            return collection.Configure<CurrencyRateServiceOptions>(o =>
            {
                var url = environment.IsLocal()
                    ? configuration["CurrencyConverter:Url"]
                    : currencyConverterOptions["url"];

                o.ServiceUrl = new Uri(url);

                var cacheLifeTimeMinutes = environment.IsLocal()
                    ? configuration["CurrencyConverter:CacheLifetimeInMinutes"]
                    : currencyConverterOptions["cacheLifetimeMinutes"];

                o.CacheLifeTime = TimeSpan.FromMinutes(int.Parse(cacheLifeTimeMinutes));
            });
        }
    }
}