using System;
using System.Diagnostics;
using System.Net;
using HappyTravel.ConsulKeyValueClient.ConfigurationProvider.Extensions;
using HappyTravel.Edo.Api.Infrastructure.Environments;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace HappyTravel.Edo.Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }


        private static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                        .UseKestrel(options =>
                        {
                            options.Listen(IPAddress.Any, EnvironmentVariableHelper.GetPort("HTDC_WEBAPI_PORT"));
                            options.Listen(IPAddress.Any, EnvironmentVariableHelper.GetPort("HTDC_METRICS_PORT"));
                            options.Listen(IPAddress.Any, EnvironmentVariableHelper.GetPort("HTDC_HEALTH_PORT"));
                        })
                        .UseSentry(options =>
                        {
                            options.Dsn = Environment.GetEnvironmentVariable("HTDC_EDO_SENTRY_ENDPOINT");
                            options.Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                            options.IncludeActivityData = true;
                            options.BeforeSend = sentryEvent =>
                            {
                                if (Activity.Current is null)
                                    return sentryEvent;

                                foreach (var (key, value) in Activity.Current.Baggage)
                                    sentryEvent.SetTag(key, value ?? string.Empty);

                                sentryEvent.SetTag("TraceId", Activity.Current.TraceId.ToString());
                                sentryEvent.SetTag("SpanId", Activity.Current.SpanId.ToString());

                                return sentryEvent;
                            };
                        })
                        .UseSetting(WebHostDefaults.SuppressStatusMessagesKey, "true");
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var environment = hostingContext.HostingEnvironment;
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

                    var consulHttpAddr = Environment.GetEnvironmentVariable("CONSUL_HTTP_ADDR") ??
                        throw new InvalidOperationException("Consul endpoint is not set");
                    var consulHttpToken = Environment.GetEnvironmentVariable("CONSUL_HTTP_TOKEN") ??
                        throw new InvalidOperationException("Consul http token is not set");

                    config.AddConsulKeyValueClient(consulHttpAddr, "edo", consulHttpToken, environment.EnvironmentName, optional: environment.IsLocal());

                    config.AddEnvironmentVariables();
                });
    }
}