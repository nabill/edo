using System;
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
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "HappyTravel.Edo.DirectApi", Version = "v1"});
            });
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HappyTravel.Edo.DirectApi v1"));
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