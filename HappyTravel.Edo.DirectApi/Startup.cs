using System;
using System.IO;
using System.Reflection;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Environments;
using HappyTravel.Edo.Data;
using HappyTravel.ErrorHandling.Extensions;
using HappyTravel.Telemetry.Extensions;
using HappyTravel.VaultClient;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

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
            
            services.AddControllers();
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
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment() || env.IsLocal())
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
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }


        private IConfiguration Configuration { get; }
        private IHostEnvironment HostEnvironment { get; }
    }
}