using HappyTravel.Edo.Api.Infrastructure.Environments;
using HappyTravel.Telemetry.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HappyTravel.Edo.DirectApi.Infrastructure.Extensions
{
    public static class ConfigureTracingExtension
    {
        internal static IServiceCollection ConfigureTracing(this IServiceCollection collection, IConfiguration configuration, IHostEnvironment environment)
        {
            return collection.AddTracing(configuration, options =>
            {
                options.ServiceName = $"{environment.ApplicationName}-{environment.EnvironmentName}";
                options.JaegerHost = environment.IsLocal()
                    ? configuration.GetValue<string>("Jaeger:AgentHost")
                    : configuration.GetValue<string>(configuration.GetValue<string>("Jaeger:AgentHost"));
                options.JaegerPort = environment.IsLocal()
                    ? configuration.GetValue<int>("Jaeger:AgentPort")
                    : configuration.GetValue<int>(configuration.GetValue<string>("Jaeger:AgentPort"));
                options.RedisEndpoint = configuration.GetValue<string>(configuration.GetValue<string>("Redis:Endpoint"));
            });
        }
    }
}