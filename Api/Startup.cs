using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using FloxDc.Bento.Responses.Middleware;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Conventions;
using HappyTravel.Edo.Api.Filters;
using HappyTravel.Edo.Api.Filters.Authorization;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Filters.Authorization.CounterpartyStatesFilters;
using HappyTravel.Edo.Api.Filters.Authorization.AgentExistingFilters;
using HappyTravel.Edo.Api.Filters.Authorization.InCounterpartyPermissionFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Infrastructure.Converters;
using HappyTravel.Edo.Api.Infrastructure.DataProviders;
using HappyTravel.Edo.Api.Infrastructure.Environments;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Payments.External.PaymentLinks;
using HappyTravel.Edo.Api.Services.Accommodations;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings;
using HappyTravel.Edo.Api.Services.CodeProcessors;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Api.Services.CurrencyConversion;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Locations;
using HappyTravel.Edo.Api.Services.Mailing;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Api.Services.Markups.Templates;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Api.Services.Payments.CreditCards;
using HappyTravel.Edo.Api.Services.Payments.External;
using HappyTravel.Edo.Api.Services.Payments.External.PaymentLinks;
using HappyTravel.Edo.Api.Services.Payments.Payfort;
using HappyTravel.Edo.Api.Services.ProviderResponses;
using HappyTravel.Edo.Api.Services.SupplierOrders;
using HappyTravel.Edo.Api.Services.Users;
using HappyTravel.Edo.Api.Services.Versioning;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.MailSender;
using HappyTravel.MailSender.Infrastructure;
using HappyTravel.StdOutLogger.Extensions;
using HappyTravel.VaultClient;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NetTopologySuite;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Polly;
using Polly.Extensions.Http;
using SendGrid.Helpers.Mail;

namespace HappyTravel.Edo.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
        }


        public IConfiguration Configuration { get; }
        public IWebHostEnvironment HostingEnvironment { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCompression();

            var serializationSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.None
            };
            JsonConvert.DefaultSettings = () => serializationSettings;

            services.AddMvcCore(options =>
                {
                    options.Conventions.Insert(0, new LocalizationConvention());
                    options.Conventions.Add(new AuthorizeControllerModelConvention());
                    options.Filters.Add(new MiddlewareFilterAttribute(typeof(LocalizationPipelineFilter)));
                    options.Filters.Add(typeof(ModelValidationFilter));
                })
                .AddAuthorization()
                .AddCors()
                .AddControllersAsServices()
                .AddFormatterMappings()
                .AddNewtonsoftJson()
                .AddApiExplorer()
                .AddCacheTagHelper()
                .AddDataAnnotations();

            services.AddCors()
                .AddLocalization()
                .AddMemoryCache()
                .AddMemoryFlow();

            var vaultOptions = new VaultOptions
            {
                BaseUrl = new Uri(EnvironmentVariableHelper.Get("Vault:Endpoint", Configuration)),
                Engine = Configuration["Vault:Engine"],
                Role = Configuration["Vault:Role"]
            };
            using var vaultClient = new VaultClient.VaultClient(vaultOptions);

            vaultClient.Login(EnvironmentVariableHelper.Get("Vault:Token", Configuration)).Wait();

            var databaseOptions = vaultClient.Get(Configuration["Edo:Database:Options"]).Result;
            var googleOptions = vaultClient.Get(Configuration["Edo:Google:Options"]).Result;
            var payfortOptions = vaultClient.Get(Configuration["Edo:Payfort:Options"]).Result;
            var payfortUrlsOptions = vaultClient.Get(Configuration["Edo:Payfort:Urls"]).Result;
            var mailSettings = vaultClient.Get(Configuration["Edo:Email:Options"]).Result;
            var administrators = JsonConvert.DeserializeObject<List<string>>(mailSettings[Configuration["Edo:Email:Administrators"]]);
            var sendGridApiKey = mailSettings[Configuration["Edo:Email:ApiKey"]];
            var senderAddress = mailSettings[Configuration["Edo:Email:SenderAddress"]];
            var agentInvitationTemplateId = mailSettings[Configuration["Edo:Email:AgentInvitationTemplateId"]];
            var administratorInvitationTemplateId = mailSettings[Configuration["Edo:Email:AdministratorInvitationTemplateId"]];
            var unknownAgentTemplateId = mailSettings[Configuration["Edo:Email:UnknownAgentBillTemplateId"]];
            var needPaymentTemplateId = mailSettings[Configuration["Edo:Email:NeedPaymentTemplateId"]];
            var bookingCancelledTemplateId = mailSettings[Configuration["Edo:Email:BookingCancelledTemplateId"]];
            var knownAgentTemplateId = mailSettings[Configuration["Edo:Email:KnownAgentBillTemplateId"]];
            var externalPaymentsMailTemplateId = mailSettings[Configuration["Edo:Email:ExternalPaymentsTemplateId"]];
            var masterAgentRegistrationMailTemplateId = mailSettings[Configuration["Edo:Email:MasterAgentRegistrationTemplateId"]];
            var regularAgentRegistrationMailTemplateId = mailSettings[Configuration["Edo:Email:RegularAgentRegistrationTemplateId"]];
            var bookingVoucherTemplateId = mailSettings[Configuration["Edo:Email:BookingVoucherTemplateId"]];
            var bookingInvoiceTemplateId = mailSettings[Configuration["Edo:Email:BookingInvoiceTemplateId"]];
            var edoPublicUrl = mailSettings[Configuration["Edo:Email:EdoPublicUrl"]];
            var currencyConverterOptions = vaultClient.Get(Configuration["CurrencyConverter:Options"]).Result;

            var paymentLinksOptions = vaultClient.Get(Configuration["PaymentLinks:Options"]).Result;

            var authorityOptions = vaultClient.Get(Configuration["Authority:Options"]).Result;
            var dataProvidersOptions = vaultClient.Get(Configuration["DataProviders:Options"]).Result;

            services.Configure<SenderOptions>(options =>
            {
                options.ApiKey = sendGridApiKey;
                options.BaseUrl = edoPublicUrl;
                options.SenderAddress = new EmailAddress(senderAddress);
            });

            services.Configure<PaymentLinkOptions>(options =>
            {
                options.ClientSettings = new ClientSettings
                {
                    Currencies = Configuration.GetSection("PaymentLinks:Currencies")
                        .Get<List<Currencies>>(),
                    ServiceTypes = Configuration.GetSection("PaymentLinks:ServiceTypes")
                        .Get<Dictionary<ServiceTypes, string>>()
                };
                options.MailTemplateId = externalPaymentsMailTemplateId;
                options.SupportedVersions = new List<Version> {new Version(0, 2)};
                options.PaymentUrlPrefix = new Uri(paymentLinksOptions["endpoint"]);
            });

            services.Configure<AgentInvitationOptions>(options =>
            {
                options.MailTemplateId = agentInvitationTemplateId;
                options.EdoPublicUrl = edoPublicUrl;
            });
            services.Configure<AdministratorInvitationOptions>(options =>
            {
                options.MailTemplateId = administratorInvitationTemplateId;
                options.EdoPublicUrl = edoPublicUrl;
            });
            services.Configure<UserInvitationOptions>(options =>
                options.InvitationExpirationPeriod = TimeSpan.FromDays(7));

            services.Configure<AgentRegistrationNotificationOptions>(options =>
            {
                options.AdministratorsEmails = administrators;
                options.MasterAgentMailTemplateId = masterAgentRegistrationMailTemplateId;
                options.RegularAgentMailTemplateId = regularAgentRegistrationMailTemplateId;
            });

            services.Configure<BookingMailingOptions>(options =>
            {
                options.VoucherTemplateId = bookingVoucherTemplateId;
                options.InvoiceTemplateId = bookingInvoiceTemplateId;
                options.BookingCancelledTemplateId = bookingCancelledTemplateId;
            });

            services.AddEntityFrameworkNpgsql().AddDbContextPool<EdoContext>(options =>
            {
                var host = databaseOptions["host"];
                var port = databaseOptions["port"];
                var password = databaseOptions["password"];
                var userId = databaseOptions["userId"];

                var connectionString = Configuration.GetConnectionString("Edo");
                options.UseNpgsql(string.Format(connectionString, host, port, userId, password), builder =>
                {
                    builder.UseNetTopologySuite();
                    builder.EnableRetryOnFailure();
                });
                options.EnableSensitiveDataLogging(false);
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }, 16);

            var apiName = Configuration["Authority:ApiName"];
            var authorityUrl = Configuration["Authority:Endpoint"];
            if (!HostingEnvironment.IsDevelopment() && !HostingEnvironment.IsLocal())
            {
                apiName = authorityOptions["apiName"];
                authorityUrl = authorityOptions["authorityUrl"];
            }

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = authorityUrl;
                    options.ApiName = apiName;
                    options.RequireHttpsMetadata = true;
                    options.SupportedTokens = SupportedTokens.Jwt;
                });

            services.AddHttpClient(HttpClientNames.OpenApiDiscovery, client => client.BaseAddress = new Uri(authorityUrl));
            services.AddHttpClient(HttpClientNames.OpenApiUserInfo);

            services.AddHttpClient(HttpClientNames.GoogleMaps, c => { c.BaseAddress = new Uri(Configuration["Edo:Google:Endpoint"]); })
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(GetDefaultRetryPolicy());

            services.AddHttpClient(SendGridMailSender.HttpClientName)
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(GetDefaultRetryPolicy());

            services.AddHttpClient(HttpClientNames.Payfort)
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(GetDefaultRetryPolicy());
            
            services.AddHttpClient(HttpClientNames.CurrencyService)
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(GetDefaultRetryPolicy());

            services.Configure<GoogleOptions>(options => { options.ApiKey = googleOptions["apiKey"]; })
                .Configure<FlowOptions>(options =>
                {
                    options.CacheKeyDelimiter = "::";
                    options.CacheKeyPrefix = "HappyTravel::Edo::Api";
                })
                .Configure<RequestLocalizationOptions>(options =>
                {
                    options.DefaultRequestCulture = new RequestCulture("en");
                    options.SupportedCultures = new[]
                    {
                        new CultureInfo("en"),
                        new CultureInfo("ar"),
                        new CultureInfo("ru")
                    };

                    options.RequestCultureProviders.Insert(0, new RouteDataRequestCultureProvider {Options = options});
                })
                .Configure<DataProviderOptions>(options =>
                {
                    var netstormingEndpoint = HostingEnvironment.IsLocal()
                        ? Configuration["DataProviders:NetstormingConnector"]
                        : dataProvidersOptions["netstormingConnector"];

                    options.Netstorming = netstormingEndpoint;
                   
                    var illusionsEndpoint = HostingEnvironment.IsLocal()
                        ? Configuration["DataProviders:Illusions"]
                        : dataProvidersOptions["illusions"];

                    options.Illusions = illusionsEndpoint;

                    var enabledConnectors = HostingEnvironment.IsLocal()
                        ? Configuration["DataProviders:EnabledConnectors"]
                        : dataProvidersOptions["enabledConnectors"];
                    
                    options.EnabledProviders = enabledConnectors
                        .Split(';')
                        .Select(c => c.Trim())
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .Select(Enum.Parse<DataProviders>)
                        .ToList();
                })
                .Configure<PayfortOptions>(options =>
                {
                    options.AccessCode = payfortOptions["access-code"];
                    options.Identifier = payfortOptions["merchant-identifier"];
                    options.ShaRequestPhrase = payfortOptions["request-phrase"];
                    options.ShaResponsePhrase = payfortOptions["response-phrase"];
                    options.PaymentUrl = payfortUrlsOptions["payment"];
                    options.TokenizationUrl = payfortUrlsOptions["tokenization"];
                    options.ReturnUrl = payfortUrlsOptions["return"];
                    options.ResultUrl = payfortUrlsOptions["result"];
                });

            services.AddSingleton(NtsGeometryServices.Instance.CreateGeometryFactory(DefaultReferenceId));

            services.AddTransient<IDataProviderClient, DataProviderClient>();
            services.AddTransient<ICountryService, CountryService>();
            services.AddTransient<IGeoCoder, GoogleGeoCoder>();
            services.AddTransient<IGeoCoder, InteriorGeoCoder>();
            services.Configure<LocationServiceOptions>(o =>
            {
                o.IsGoogleGeoCoderDisabled = bool.TryParse(googleOptions["disabled"], out var disabled) && disabled;
            });

            services.AddSingleton<IVersionService, VersionService>();

            services.AddTransient<ILocationService, LocationService>();
            services.AddTransient<ICounterpartyService, CounterpartyService>();
            services.AddTransient<IAgentService, AgentService>();
            services.AddTransient<IAgentRegistrationService, AgentRegistrationService>();
            services.AddTransient<IAccountPaymentService, AccountPaymentService>();
            services.AddTransient<IPaymentSettingsService, PaymentSettingsService>();
            services.AddTransient<IBookingPaymentService, BookingPaymentService>();
            services.AddTransient<IAccommodationService, AccommodationService>();
            services.AddScoped<IAgentContext, HttpBasedAgentContext>();
            services.AddScoped<IAgentContextInternal, HttpBasedAgentContext>();
            services.AddHttpContextAccessor();
            services.AddSingleton<IDateTimeProvider, DefaultDateTimeProvider>();
            services.AddSingleton<IAvailabilityResultsCache, AvailabilityResultsCache>();
            services.AddTransient<IBookingManager, BookingManager>();
            services.AddTransient<ITagProcessor, TagProcessor>();

            services.AddTransient<IAgentInvitationService, AgentInvitationService>();
            services.AddSingleton<IMailSender, SendGridMailSender>();
            services.AddSingleton<ITokenInfoAccessor, TokenInfoAccessor>();
            services.AddTransient<IAccountBalanceAuditService, AccountBalanceAuditService>();
            services.AddTransient<ICreditCardAuditService, CreditCardAuditService>();

            services.AddTransient<IAccountManagementService, AccountManagementService>();
            services.AddScoped<IAdministratorContext, HttpBasedAdministratorContext>();
            services.AddScoped<IServiceAccountContext, HttpBasedServiceAccountContext>();

            services.AddTransient<IUserInvitationService, UserInvitationService>();
            services.AddTransient<IAdministratorInvitationService, AdministratorInvitationService>();
            services.AddTransient<IExternalAdminContext, ExternalAdminContext>();

            services.AddTransient<IAdministratorRegistrationService, AdministratorRegistrationService>();
            services.AddScoped<IManagementAuditService, ManagementAuditService>();

            services.AddScoped<IEntityLocker, EntityLocker>();
            services.AddTransient<IAccountPaymentProcessingService, AccountPaymentProcessingService>();

            services.AddTransient<IPayfortService, PayfortService>();
            services.AddTransient<ICreditCardsManagementService, CreditCardsManagementService>();
            services.AddTransient<IPayfortSignatureService, PayfortSignatureService>();

            services.AddTransient<IMarkupService, MarkupService>();

            services.AddSingleton<IMarkupPolicyTemplateService, MarkupPolicyTemplateService>();
            services.AddScoped<IMarkupPolicyManager, MarkupPolicyManager>();

            services.AddScoped<ICurrencyRateService, CurrencyRateService>();
            services.AddScoped<ICurrencyConverterService, CurrencyConverterService>();

            services.AddTransient<ISupplierOrderService, SupplierOrderService>();
            services.AddTransient<IMarkupLogger, MarkupLogger>();

            services.AddSingleton<IJsonSerializer, NewtonsoftJsonSerializer>();
            services.AddTransient<IAgentSettingsManager, AgentSettingsManager>();

            services.AddTransient<IPaymentLinkService, PaymentLinkService>();
            services.AddTransient<IPaymentLinksProcessingService, PaymentLinksProcessingService>();
            services.AddTransient<IPaymentCallbackDispatcher, PaymentCallbackDispatcher>();
            services.AddTransient<IAgentPermissionManagementService, AgentPermissionManagementService>();
            services.AddTransient<IPermissionChecker, PermissionChecker>();
            services.AddTransient<IPaymentNotificationService, PaymentNotificationService>();
            services.AddTransient<IBookingMailingService, BookingMailingService>();
            services.AddTransient<IPaymentHistoryService, PaymentHistoryService>();
            services.AddTransient<IBookingDocumentsService, BookingDocumentsService>();
            services.AddTransient<INetstormingResponseService, NetstormingResponseService>();
            services.AddTransient<IBookingAuditLogService, BookingAuditLogService>();
            services.AddTransient<IDataProviderFactory, DataProviderFactory>();
            services.AddTransient<IAvailabilityService, AvailabilityService>();
            services.AddTransient<IBookingService, BookingService>();
            services.AddTransient<IBookingsProcessingService, BookingsProcessingService>();
            services.AddTransient<IProviderRouter, ProviderRouter>();

            services.AddSingleton<IAuthorizationPolicyProvider, CustomAuthorizationPolicyProvider>();
            services.AddTransient<IAuthorizationHandler, InCounterpartyPermissionAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, MinCounterpartyStateAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, AdministratorPermissionsAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, AgentRequiredAuthorizationHandler>();

            services.AddTransient<ICreditCardPaymentProcessingService, CreditCardPaymentProcessingService>();
            services.AddTransient<ICreditCardMoneyAuthorizationService, CreditCardMoneyAuthorizationService>();
            services.AddTransient<ICreditCardMoneyCaptureService, CreditCardMoneyCaptureService>();
            services.AddTransient<IPayfortResponseParser, PayfortResponseParser>();
            
            // Default behaviour allows not authenticated requests to be checked by authorization policies.
            // Special wrapper returns Forbid result for them.
            // More information: https://github.com/dotnet/aspnetcore/issues/4656
            services.AddTransient<IPolicyEvaluator, ForbidUnauthenticatedPolicyEvaluator>();
            // Default policy evaluator needs to be registered as dependency of ForbidUnauthenticatedPolicyEvaluator.
            services.AddTransient<PolicyEvaluator>();
            
            services.Configure<PaymentNotificationOptions>(po =>
            {
                po.KnownAgentTemplateId = knownAgentTemplateId;
                po.UnknownAgentTemplateId = unknownAgentTemplateId;
                po.NeedPaymentTemplateId = needPaymentTemplateId;
            });

            services.Configure<CurrencyRateServiceOptions>(o =>
            {
                var url = HostingEnvironment.IsLocal()
                    ? Configuration["CurrencyConverter:Url"]
                    : currencyConverterOptions["url"];

                o.ServiceUrl = new Uri(url);

                var cacheLifeTimeMinutes = HostingEnvironment.IsLocal()
                    ? Configuration["CurrencyConverter:CacheLifetimeInMinutes"]
                    : currencyConverterOptions["cacheLifetimeMinutes"];

                o.CacheLifeTime = TimeSpan.FromMinutes(int.Parse(cacheLifeTimeMinutes));
            });

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
                options.SwaggerDoc("v1.0", new OpenApiInfo {Title = "HappyTravel.com Edo API", Version = "v1.0"});

                var xmlCommentsFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlCommentsFilePath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFileName);
                options.IncludeXmlComments(xmlCommentsFilePath);
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        Array.Empty<string>()
                    }
                });
            });
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            Infrastructure.Logging.AppLogging.LoggerFactory = loggerFactory;
            
            app.UseBentoExceptionHandler(env.IsProduction());
            
            app.UseHttpContextLogging(
                options => options.IgnoredPaths = new HashSet<string> {"/health"}
            );

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1.0/swagger.json", "HappyTravel.com Edo API");
                options.RoutePrefix = string.Empty;
            });

            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());

            app.UseResponseCompression();

            var headersOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor,
                RequireHeaderSymmetry = false,
                ForwardLimit = null
            };
            headersOptions.KnownNetworks.Clear();
            headersOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(headersOptions);
            app.UseAuthentication();
            
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapControllers();
            });
        }


        private IAsyncPolicy<HttpResponseMessage> GetDefaultRetryPolicy()
        {
            var jitter = new Random();

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, attempt
                    => TimeSpan.FromMilliseconds(Math.Pow(500, attempt)) + TimeSpan.FromMilliseconds(jitter.Next(0, 100)));
        }


        private const int DefaultReferenceId = 4326;
    }
}