using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Serialization;
using FluentValidation.AspNetCore;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Environments;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Common.Infrastructure.Options;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.DirectApi.Infrastructure.Extensions;
using HappyTravel.Edo.DirectApi.Infrastructure.Middlewares;
using HappyTravel.Edo.DirectApi.Services;
using HappyTravel.Edo.DirectApi.Services.AvailabilitySearch;
using HappyTravel.Edo.DirectApi.Services.Bookings;
using HappyTravel.Edo.DirectApi.Services.Overriden;
using HappyTravel.ErrorHandling.Extensions;
using HappyTravel.VaultClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;
using AccommodationService = HappyTravel.Edo.DirectApi.Services.Static.AccommodationService;
using WideAvailabilitySearchService = HappyTravel.Edo.DirectApi.Services.AvailabilitySearch.WideAvailabilitySearchService;

namespace HappyTravel.Edo.DirectApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            Configuration = configuration;
            HostEnvironment = hostEnvironment;
        }

        
        public void ConfigureServices(IServiceCollection services)
        {
            using var vaultClient = new VaultClient.VaultClient(new VaultOptions
            {
                BaseUrl = new Uri(EnvironmentVariableHelper.Get("Vault:Endpoint", Configuration)),
                Engine = Configuration["Vault:Engine"],
                Role = Configuration["Vault:Role"]
            });
            vaultClient.Login(EnvironmentVariableHelper.Get("Vault:Token", Configuration)).GetAwaiter().GetResult();
            
            var authorityOptions = Configuration.GetSection("Authority").Get<AuthorityOptions>();
            authorityOptions.Audience = "direct_api"; // override edo value

            services.AddHealthChecks()
                .AddDbContextCheck<EdoContext>()
                .AddRedis(EnvironmentVariableHelper.Get("Redis:Endpoint", Configuration));
            
            services.ConfigureAuthentication(authorityOptions);
            services.AddControllers()
                .AddNewtonsoftJson(opts => opts.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter()))
                .AddJsonOptions(opts => opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
                .AddFluentValidation(fv =>
                {
                    fv.DisableDataAnnotationsValidation = true;
                    fv.ImplicitlyValidateRootCollectionElements = true;
                    fv.ImplicitlyValidateChildProperties = true;
                    fv.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());
                });
            services.AddResponseCompression();
            services.ConfigureTracing(Configuration, HostEnvironment);
            services.AddProblemDetailsErrorHandling();
            services.ConfigureApiVersioning();
            services.ConfigureSwagger();
            services.ConfigureCache(Configuration);
            services.ConfigureHttpClients(Configuration, vaultClient, authorityOptions.AuthorityUrl ?? string.Empty);
            services.ConfigureServiceOptions(Configuration, vaultClient);
            services.ConfigureUserEventLogging(Configuration, vaultClient);
            services.AddServices(HostEnvironment, Configuration, vaultClient);
            services.AddSignalR().AddStackExchangeRedis(EnvironmentVariableHelper.Get("Redis:Endpoint", Configuration));

            // override services
            services.AddTransient<AccommodationAvailabilitiesService>();
            services.AddTransient<AccommodationService>();
            services.AddTransient<IAgentContextService, AgentContextService>();
            services.AddTransient<BookingCancellationService>();
            services.AddTransient<IBookingEvaluationService, DirectApiBookingEvaluationService>();
            services.AddTransient<INotificationService, EdoDummyNotificationService>();
            services.AddTransient<ValuationService>();
            services.AddTransient<WideAvailabilitySearchService>();
            services.AddTransient<BookingInfoService>();
            services.AddTransient<BookingCreationService>();
            services.AddTransient<IBookingRegistrationService, DirectApiBookingRegistrationService>();
            services.AddTransient<ClientReferenceCodeValidationService>();
            services.ConfigureWideAvailabilityStorage(HostEnvironment, Configuration, vaultClient);
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (!env.IsProduction())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseReDoc(c =>
                {
                    c.SpecUrl = "/swagger/direct-api/swagger.json";
                    c.RoutePrefix = string.Empty;
                    c.DocumentTitle = "HappyTravel Direct API";
                    c.DisableSearch();
                    c.HideDownloadButton();
                    c.ExpandResponses("");
                });
            }
            
            var logger = loggerFactory.CreateLogger<Startup>();
            
            app.UseProblemDetailsExceptionHandler(env, logger);
            app.UseResponseCompression();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseHttpMetrics();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseClientRequestLogging();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics().RequireHost($"*:{EnvironmentVariableHelper.GetPort("HTDC_METRICS_PORT")}");
                endpoints.MapHealthChecks("/health").RequireHost($"*:{EnvironmentVariableHelper.GetPort("HTDC_HEALTH_PORT")}");
            });
        }


        private IConfiguration Configuration { get; }
        private IHostEnvironment HostEnvironment { get; }
    }
}