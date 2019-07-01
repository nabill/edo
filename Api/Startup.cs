using System;
using System.IO;
using System.Reflection;
using FloxDc.Bento.Responses.Middleware;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Services.Locations;
using HappyTravel.Edo.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            services.AddHealthChecks();

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

            services.AddEntityFrameworkNpgsql().AddDbContextPool<EdoContext>(options =>
            {
                var host = GetFromEnvironment("Edo:Database:Host");
                var port = GetFromEnvironment("Edo:Database:Port");
                var password = GetFromEnvironment("Edo:Database:Password");
                var userId = GetFromEnvironment("Edo:Database:UserId");

                var connectionString = Configuration.GetConnectionString("Edo");
                options.UseNpgsql(string.Format(connectionString, host, port, userId, password));
                options.EnableSensitiveDataLogging(false);
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }, 16);

            services.AddHttpClient("google-maps", c =>
            {
                c.BaseAddress = new Uri(Configuration["Edo:Google:Endpoint"]);
            });

            services.Configure<GoogleOptions>(o => { o.ApiKey = GetFromEnvironment("Edo:Google:ApiKey"); });

            services.AddTransient<IGeocoder, GoogleGeocoder>();
            services.AddTransient<ILocationService, LocationService>();
            
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


        private string GetFromEnvironment(string key)
        {
            var environmentVariable = Configuration[key];
            if (environmentVariable is null)
                throw new Exception($"Couldn't obtain the value for '{key}' configuration key.");

            return Environment.GetEnvironmentVariable(environmentVariable);
        }
    }
}
