using System;
using System.IO;
using System.Net;
using System.Reflection;
using CacheFlow.Json.Extensions;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.CurrencyConverter;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Environments;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Infrastructure.SupplierConnectors;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Api.Services.CurrencyConversion;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Api.Services.Markups.Templates;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.DirectApi.Services;
using HappyTravel.ErrorHandling.Extensions;
using HappyTravel.Telemetry.Extensions;
using HappyTravel.VaultClient;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;

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
            
            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = authorityOptions["authorityUrl"];
                    options.ApiName = authorityOptions["apiName"];
                    options.RequireHttpsMetadata = true;
                    options.SupportedTokens = SupportedTokens.Jwt;
                });

            services.AddControllers().AddNewtonsoftJson();
            services.AddResponseCompression();
            services.AddTracing(Configuration, options =>
            {
                options.ServiceName = $"{HostEnvironment.ApplicationName}-{HostEnvironment.EnvironmentName}";
                options.JaegerHost = HostEnvironment.IsLocal()
                    ? Configuration.GetValue<string>("Jaeger:AgentHost")
                    : Configuration.GetValue<string>(Configuration.GetValue<string>("Jaeger:AgentHost"));
                options.JaegerPort = HostEnvironment.IsLocal()
                    ? Configuration.GetValue<int>("Jaeger:AgentPort")
                    : Configuration.GetValue<int>(Configuration.GetValue<string>("Jaeger:AgentPort"));
                options.RedisEndpoint = Configuration.GetValue<string>(Configuration.GetValue<string>("Redis:Endpoint"));
            });
            services.AddHealthChecks()
                .AddDbContextCheck<EdoContext>()
                .AddRedis(EnvironmentVariableHelper.Get("Redis:Endpoint", Configuration))
                .AddCheck<ControllerResolveHealthCheck>(nameof(ControllerResolveHealthCheck));
            
            services.AddProblemDetailsErrorHandling();

            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = false;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });
            
            services.AddVersionedApiExplorer(options =>
            {
                options.SubstitutionFormat = "V.v";
                options.SubstituteApiVersionInUrl = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
            });
            
            services.AddSwaggerGen(c =>
            {
                c.DocInclusionPredicate((_, apiDesc) =>
                {
                    // Hide apis from other assemblies
                    var apiName = apiDesc.ActionDescriptor.DisplayName;
                    var currentName = typeof(Startup).Assembly.GetName().Name;
                    return apiName.StartsWith(currentName);
                });
                c.SwaggerDoc("direct-api", new OpenApiInfo
                {
                    Title = "HappyTravel Api",
                    Description = "HappyTravel API for searching and booking hotels"
                });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath, true);
            });
            
            var dbOptions = vaultClient.Get(Configuration["Database:Options"]).GetAwaiter().GetResult();
            services.AddDbContextPool<EdoContext>(o =>
            {
                var host = dbOptions["host"];
                var port = dbOptions["port"];
                var name = dbOptions["userId"];
                var password = dbOptions["password"];
                var user = dbOptions["userId"];
                
                var connectionString = Configuration.GetConnectionString("Edo");
                o.UseNpgsql(string.Format(connectionString, host, port, name, user, password));
                o.EnableSensitiveDataLogging(false);
                o.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }, 16);
            
            var currencyConverterOptions = vaultClient.Get(Configuration["CurrencyConverter:Options"]).GetAwaiter().GetResult();
            services.Configure<CurrencyRateServiceOptions>(o =>
            {
                var url = HostEnvironment.IsLocal()
                    ? Configuration["CurrencyConverter:Url"]
                    : currencyConverterOptions["url"];

                o.ServiceUrl = new Uri(url);

                var cacheLifeTimeMinutes = HostEnvironment.IsLocal()
                    ? Configuration["CurrencyConverter:CacheLifetimeInMinutes"]
                    : currencyConverterOptions["cacheLifetimeMinutes"];

                o.CacheLifeTime = TimeSpan.FromMinutes(int.Parse(cacheLifeTimeMinutes));
            });

            services
                .AddMemoryCache()
                .AddMemoryFlow()
                .AddStackExchangeRedisCache(options =>
                {
                    var host = EnvironmentVariableHelper.Get("Redis:Endpoint", Configuration);
                    options.ConfigurationOptions = new ConfigurationOptions
                    {
                        EndPoints = {new DnsEndPoint(host, 6379)},
                        AsyncTimeout = 15000, // set to 15 seconds before we stop storing large objects in Redis
                    };
                })
                .AddDoubleFlow()
                .AddCacheFlowJsonSerialization();

            services.ConfigureHttpClients(Configuration, HostEnvironment, vaultClient, authorityOptions["authorityUrl"]);
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
            services.AddTransient<IConnectorClient, ConnectorClient>();
            services.AddSingleton<IConnectorSecurityTokenManager, ConnectorSecurityTokenManager>();
            services.AddHostedService<MarkupPolicyStorageUpdater>();
            services.AddSingleton<IMarkupPolicyStorage, MarkupPolicyStorage>();

            services.AddTransient<WideSearchService>();
            
            // override Edo service
            services.AddTransient<IWideAvailabilityStorage, DirectApiRedisWideAvailabilityStorage>();
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