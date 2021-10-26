using System;
using System.Security.Cryptography.X509Certificates;
using Elasticsearch.Net;
using HappyTravel.VaultClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.Edo.DirectApi.Infrastructure.Extensions
{
    internal static class ConfigureUserEventLoggingExtension
    {
        public static IServiceCollection ConfigureUserEventLogging(this IServiceCollection collection, IConfiguration configuration, IVaultClient vaultClient)
        {
            var elasticOptions = vaultClient.Get(configuration["UserEvents:ElasticSearch"]).GetAwaiter().GetResult();
            
            return collection.AddSingleton<IElasticLowLevelClient>(provider =>
            {
                var settings = new ConnectionConfiguration(new Uri(elasticOptions["endpoint"]))
                    .BasicAuthentication(elasticOptions["username"], elasticOptions["password"])
                    .ServerCertificateValidationCallback((o, certificate, chain, errors) => true)
                    .ClientCertificate(new X509Certificate2(Convert.FromBase64String(elasticOptions["certificate"])));
                var client = new ElasticLowLevelClient(settings);

                return client;
            });
        }
    }
}