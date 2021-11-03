using System.Net;
using CacheFlow.Json.Extensions;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure.Environments;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace HappyTravel.Edo.DirectApi.Infrastructure.Extensions
{
    internal static class ConfigureCacheExtension
    {
        public static IServiceCollection ConfigureCache(this IServiceCollection collection, IConfiguration configuration)
        {
            return collection
                .AddMemoryCache()
                .AddMemoryFlow()
                .AddStackExchangeRedisCache(options =>
                {
                    var host = EnvironmentVariableHelper.Get("Redis:Endpoint", configuration);
                    options.ConfigurationOptions = new ConfigurationOptions
                    {
                        EndPoints = {new DnsEndPoint(host, 6379)},
                        AsyncTimeout = 15000, // set to 15 seconds before we stop storing large objects in Redis
                    };
                })
                .AddDoubleFlow()
                .AddCacheFlowJsonSerialization();
        }
    }
}