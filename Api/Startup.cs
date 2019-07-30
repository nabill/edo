using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using FloxDc.Bento.Responses.Middleware;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Services.Availabilities;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Api.Services.Locations;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Data;
using HappyTravel.VaultClient;
using HappyTravel.VaultClient.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite;
using Polly;
using Polly.Extensions.Http;
using Swashbuckle.AspNetCore.Swagger;

namespace HappyTravel.Edo.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCompression();

            services.AddMvcCore()
                .AddAuthorization()
                .AddCors()
                .AddControllersAsServices()
                .AddFormatterMappings()
                .AddJsonFormatters()
                .AddApiExplorer()
                .AddCacheTagHelper()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddCors()
                .AddLocalization()
                .AddMemoryCache()
                .AddMemoryFlow();

            /*services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "https://localhost:5443";
                    options.ApiName = "edo";
                    options.EnableCaching = true;
                    options.CacheDuration = TimeSpan.FromMinutes(10);

                    options.RequireHttpsMetadata = false;
                });*/

            Dictionary<string, string> databaseOptions;
            Dictionary<string, string> googleOptions;
            services.AddVaultClient(o =>
            {
                o.Engine = Configuration["Vault:Engine"];
                o.Role = Configuration["Vault:Role"];
                o.Url = new Uri(Configuration["Vault:Endpoint"]);
            });
            var serviceProvider = services.BuildServiceProvider();
            using (var vaultClient = serviceProvider.GetService<IVaultClient>())
            {
                vaultClient.Login(GetFromEnvironment("Vault:Token"));

                databaseOptions = vaultClient.Get(Configuration["Edo:Database:Options"]).Result;
                googleOptions = vaultClient.Get(Configuration["Edo:Google:Options"]).Result;
            }

            services.AddEntityFrameworkNpgsql().AddDbContextPool<EdoContext>(options =>
            {
                var host = databaseOptions["host"];
                var port = databaseOptions["port"];
                var password = databaseOptions["password"];
                var userId = databaseOptions["userId"];

                var connectionString = Configuration.GetConnectionString("Edo");
                options.UseNpgsql(string.Format(connectionString, host, port, userId, password), builder => builder.UseNetTopologySuite());
                options.EnableSensitiveDataLogging(false);
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }, 16);

            services.AddHttpClient(HttpClientNames.GoogleMaps, c =>
                {
                    c.BaseAddress = new Uri(Configuration["Edo:Google:Endpoint"]);
                })
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(GetDefaultRetryPolicy());

            services.AddHttpClient(HttpClientNames.NetstormingConnector, client =>
            {
                client.BaseAddress = new Uri(Configuration["HttpClientUrls:NetstormingConnector"]);
            });

            services.Configure<GoogleOptions>(o => { o.ApiKey = googleOptions["apiKey"]; });

            services.AddSingleton(NtsGeometryServices.Instance.CreateGeometryFactory(DefaultReferenceId));

            services.AddTransient<IGeoCoder, GoogleGeoCoder>();
            services.AddTransient<ILocationService, LocationService>();
            services.AddTransient<IAvailabilityService, AvailabilityService>();
            services.AddTransient<ICompanyService, CompanyService>();
            services.AddTransient<ICustomerService, CustomerService>();
            services.AddTransient<IRegistrationService, RegistrationService>();
            services.AddTransient<IPaymentService, PaymentService>();

            services.AddHealthChecks()
                .AddDbContextCheck<EdoContext>();
            
            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = false;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1.0", new Info {Title = "HappyTravel.com Edo API", Version = "v1.0" });

                var xmlCommentsFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlCommentsFilePath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFileName);
                options.IncludeXmlComments(xmlCommentsFilePath);
            });
        }


        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseBentoExceptionHandler(env.IsProduction());
            
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1.0/swagger.json", "HappyTravel.com Edo API");
                options.RoutePrefix = string.Empty;
            });

            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyHeader());
            app.UseHealthChecks("/health");
            app.UseResponseCompression();

            //app.UseAuthentication();
            app.UseMvc();
        }


        private IAsyncPolicy<HttpResponseMessage> GetDefaultRetryPolicy()
        {
            var jitter = new Random();

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, attempt 
                    => TimeSpan.FromMilliseconds(Math.Pow(500, attempt)) + TimeSpan.FromMilliseconds(jitter.Next(0, 100)));
        }


        private string GetFromEnvironment(string key)
        {
            var environmentVariable = Configuration[key];
            if (environmentVariable is null)
                throw new Exception($"Couldn't obtain the value for '{key}' configuration key.");

            return Environment.GetEnvironmentVariable(environmentVariable);
        }


        private const int DefaultReferenceId = 4326;
    }
}
