using System;
using HappyTravel.CurrencyConverter;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Analytics;
using HappyTravel.Edo.Api.Infrastructure.Environments;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Infrastructure.SupplierConnectors;
using HappyTravel.Edo.Api.NotificationCenter.Infrastructure;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Analytics;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Api.Services.CurrencyConversion;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Api.Services.Markups.Templates;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.DirectApi.Infrastructure.Extensions;
using HappyTravel.Edo.DirectApi.Services;
using HappyTravel.ErrorHandling.Extensions;
using HappyTravel.VaultClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
            
            var authorityOptions = vaultClient.Get(Configuration["Authority:Options"]).GetAwaiter().GetResult();

            services.AddHealthChecks()
                .AddDbContextCheck<EdoContext>()
                .AddRedis(EnvironmentVariableHelper.Get("Redis:Endpoint", Configuration))
                .AddCheck<ControllerResolveHealthCheck>(nameof(ControllerResolveHealthCheck));
            
            services.ConfigureAuthentication(authorityOptions);
            services.AddControllers().AddNewtonsoftJson();
            services.AddResponseCompression();
            services.ConfigureTracing(Configuration, HostEnvironment);
            services.AddProblemDetailsErrorHandling();
            services.ConfigureApiVersioning();
            services.ConfigureSwagger();
            services.ConfigureDatabase(Configuration, vaultClient);
            services.ConfigureCurrencyConversion(Configuration, HostEnvironment, vaultClient);
            services.ConfigureCache(Configuration);
            services.ConfigureHttpClients(Configuration, HostEnvironment, vaultClient, authorityOptions["authorityUrl"]);
            services.ConfigureUserEventLogging(Configuration, vaultClient);
            services.AddNotificationCenter(EnvironmentVariableHelper.Get("Redis:Endpoint", Configuration));
            services.Configure<SupplierOptions>(Configuration.GetSection("Suppliers"));
            services.AddTransient<IAgentContextService, HttpBasedAgentContextService>();
            services.AddTransient<ITokenInfoAccessor, TokenInfoAccessor>();
            services.AddTransient<IAccommodationBookingSettingsService, AccommodationBookingSettingsService>();
            services.AddTransient<IAgentSystemSettingsService, AgentSystemSettingsService>();
            services.AddTransient<IAgencySystemSettingsService, AgencySystemSettingsService>();
            services.AddTransient<ICounterpartySystemSettingsService, CounterpartySystemSettingsService>();
            services.AddTransient<IWideAvailabilitySearchStateStorage, WideAvailabilitySearchStateStorage>();
            services.AddTransient<IMultiProviderAvailabilityStorage, MultiProviderAvailabilityStorage>();
            services.AddTransient<IAvailabilitySearchAreaService, AvailabilitySearchAreaService>();
            services.AddTransient<IAccommodationMapperClient, AccommodationMapperClient>();
            services.AddTransient<IWideAvailabilityPriceProcessor, WideAvailabilityPriceProcessor>();
            services.AddTransient<ISupplierConnectorManager, SupplierConnectorManager>();
            services.AddTransient<IDateTimeProvider, DefaultDateTimeProvider>();
            services.AddTransient<IPriceProcessor, PriceProcessor>();
            services.AddTransient<IMarkupService, MarkupService>();
            services.AddTransient<IMarkupPolicyService, MarkupPolicyService>();
            services.AddTransient<IDiscountFunctionService, DiscountFunctionService>();
            services.AddTransient<IMarkupPolicyTemplateService, MarkupPolicyTemplateService>();
            services.AddTransient<ICurrencyRateService, CurrencyRateService>();
            services.AddTransient<ICurrencyConverterService, CurrencyConverterService>();
            services.AddTransient<ICurrencyConverterFactory, CurrencyConverterFactory>();
            services.AddTransient<IAgencyService, AgencyService>();
            services.AddTransient<IAdminAgencyManagementService, AdminAgencyManagementService>();
            services.AddTransient<IManagementAuditService, ManagementAuditService>();
            services.AddTransient<IAdministratorContext, HttpBasedAdministratorContext>();
            services.AddTransient<IWideAvailabilitySearchService, WideAvailabilitySearchService>();
            services.AddTransient<IBookingAnalyticsService, BookingAnalyticsService>();
            services.AddTransient<IAnalyticsService, ElasticAnalyticsService>();
            services.AddTransient<IConnectorClient, ConnectorClient>();
            services.AddTransient<IWideAvailabilityAccommodationsStorage, WideAvailabilityAccommodationsStorage>();
            services.AddSingleton<IConnectorSecurityTokenManager, ConnectorSecurityTokenManager>();
            services.AddHostedService<MarkupPolicyStorageUpdater>();
            services.AddSingleton<IMarkupPolicyStorage, MarkupPolicyStorage>();
            services.AddTransient<IRoomSelectionService, RoomSelectionService>();
            services.AddTransient<IBookingAnalyticsService, BookingAnalyticsService>();
            services.AddTransient<IAnalyticsService, ElasticAnalyticsService>();
            services.AddTransient<IRoomSelectionPriceProcessor, RoomSelectionPriceProcessor>();
            services.AddTransient<IRoomSelectionStorage, RoomSelectionStorage>();
            services.AddTransient<WideSearchService>();
            services.AddTransient<AccommodationAvailabilitiesService>();
            services.ConfigureWideAvailabilityStorage(Configuration, vaultClient);
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
            app.UseHealthChecks("/health");
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }


        private IConfiguration Configuration { get; }
        private IHostEnvironment HostEnvironment { get; }
    }
}