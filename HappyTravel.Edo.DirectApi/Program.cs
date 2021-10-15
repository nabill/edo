using System;
using System.Diagnostics;
using HappyTravel.ConsulKeyValueClient.ConfigurationProvider.Extensions;
using HappyTravel.Edo.Api.Infrastructure.Environments;
using HappyTravel.StdOutLogger.Extensions;
using HappyTravel.StdOutLogger.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.DirectApi
{
    public class Program
    {
        public static void Main(string[] args) 
            => CreateHostBuilder(args).Build().Run();


        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                        .UseKestrel()
                        .UseSentry(options =>
                        {
                            options.Dsn = Environment.GetEnvironmentVariable("HTDC_EDO_SENTRY_ENDPOINT");
                            options.Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                            options.IncludeActivityData = true;
                            options.BeforeSend = sentryEvent =>
                            {
                                foreach (var (key, value) in OpenTelemetry.Baggage.Current)
                                    sentryEvent.SetTag(key, value);

                                sentryEvent.SetTag("TraceId", Activity.Current?.TraceId.ToString() ?? string.Empty);
                                sentryEvent.SetTag("SpanId", Activity.Current?.SpanId.ToString() ?? string.Empty);

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
                    
                    var consulHttpAddr = Environment.GetEnvironmentVariable("CONSUL_HTTP_ADDR") ?? throw new InvalidOperationException("Consul endpoint is not set");
                    var consulHttpToken = Environment.GetEnvironmentVariable("CONSUL_HTTP_TOKEN") ?? throw new InvalidOperationException("Consul http token is not set");

                    config.AddConsulKeyValueClient(consulHttpAddr, "direct-api", consulHttpToken, environment.EnvironmentName, optional: environment.IsLocal());
                    
                    config.AddEnvironmentVariables();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders()
                        .AddConfiguration(hostingContext.Configuration.GetSection("Logging"));

                    var env = hostingContext.HostingEnvironment;

                    if (env.IsLocal())
                        logging.AddConsole();
                    else
                    {
                        logging.AddStdOutLogger(setup =>
                        {
                            setup.IncludeScopes = true;
                            setup.RequestIdHeader = Constants.DefaultRequestIdHeader;
                            setup.UseUtcTimestamp = true;
                        });
                        logging.AddSentry();
                    }
                });
    }
}