using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using HappyTravel.AmazonS3Client.Extensions;
using HappyTravel.Edo.Api.Filters.Authorization;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Filters.Authorization.AgentExistingFilters;
using HappyTravel.Edo.Api.Filters.Authorization.AgencyVerificationStatesFilters;
using HappyTravel.Edo.Api.Filters.Authorization.InAgencyPermissionFilters;
using HappyTravel.Edo.Api.Filters.Authorization.ServiceAccountFilters;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Infrastructure.Converters;
using HappyTravel.Edo.Api.Infrastructure.Environments;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Infrastructure.SupplierConnectors;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.CodeProcessors;
using HappyTravel.Edo.Api.Services.Company;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Api.Services.CurrencyConversion;
using HappyTravel.Edo.Api.Services.Documents;
using HappyTravel.Edo.Api.Services.Locations;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Api.Services.Markups.Templates;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Api.Services.Payments.CreditCards;
using HappyTravel.Edo.Api.Services.Payments.External;
using HappyTravel.Edo.Api.Services.Payments.External.PaymentLinks;
using HappyTravel.Edo.Api.Services.Payments.Offline;
using HappyTravel.Edo.Api.Services.Payments.Payfort;
using HappyTravel.Edo.Api.Services.SupplierOrders;
using HappyTravel.Edo.Api.Services.Versioning;
using HappyTravel.Edo.Data;
using HappyTravel.Geography;
using HappyTravel.MailSender;
using HappyTravel.MailSender.Infrastructure;
using HappyTravel.MailSender.Models;
using HappyTravel.Money.Enums;
using HappyTravel.VaultClient;
using IdentityServer4.AccessTokenValidation;
using LocationNameNormalizer.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite;
using Polly;
using Polly.Extensions.Http;
using Amazon;
using Amazon.S3;
using Elasticsearch.Net;
using HappyTravel.CurrencyConverter.Extensions;
using HappyTravel.CurrencyConverter.Infrastructure;
using HappyTravel.Edo.Api.AdministratorServices.Invitations;
using HappyTravel.Edo.Api.AdministratorServices.Mapper.AccommodationManagementServices;
using HappyTravel.Edo.Api.Infrastructure.Analytics;
using HappyTravel.Edo.Api.Infrastructure.Invitations;
using HappyTravel.Edo.Api.Infrastructure.Locking;
using HappyTravel.Edo.Api.Infrastructure.MongoDb.Extensions;
using HappyTravel.Edo.Api.Models.Reports;
using HappyTravel.Edo.Api.Models.Reports.DirectConnectivityReports;
using HappyTravel.Edo.Api.Services;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.BatchProcessing;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution.Flows;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Documents;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.ResponseProcessing;
using HappyTravel.Edo.Api.Services.Analytics;
using HappyTravel.Edo.Api.Services.ApiClients;
using HappyTravel.Edo.Api.Services.Files;
using HappyTravel.Edo.Api.Services.Invitations;
using HappyTravel.Edo.Api.Services.Payments.NGenius;
using HappyTravel.Edo.Api.Services.Reports;
using HappyTravel.Edo.Api.Services.Reports.Converters;
using HappyTravel.Edo.Api.Services.Reports.RecordManagers;
using HappyTravel.Edo.Api.Services.SupplierResponses;
using IdentityModel.Client;
using Prometheus;
using HappyTravel.Edo.Api.Services.PropertyOwners;
using HappyTravel.Edo.CreditCards.Models;
using HappyTravel.Edo.CreditCards.Options;
using HappyTravel.Edo.CreditCards.Services;
using HappyTravel.SupplierOptionsProvider;
using HappyTravel.VccServiceClient.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Hosting;
using ProtoBuf.Grpc.ClientFactory;
using StackExchange.Redis;
using Tsutsujigasaki.GrpcContracts.Services;
using Api.AdministratorServices.Locations;
using HappyTravel.Edo.Api.Services.Messaging;
using NATS.Client;
using Api.Services.Markups.Notifications;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using Microsoft.Extensions.Logging;
using Api.Services.Markups;
using Api.Infrastructure.Options;
using Api.AdministratorServices;
using HappyTravel.Edo.Common.Infrastructure.Options;
using HappyTravel.HttpRequestLogger;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureAuthentication(this IServiceCollection services, AuthorityOptions authorityOptions)
        {
            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = authorityOptions.AuthorityUrl;
                    options.Audience = authorityOptions.Audience;
                    options.RequireHttpsMetadata = true;
                    options.AutomaticRefreshInterval = authorityOptions.AutomaticRefreshInterval;
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var func = !context.Request.Path.StartsWithSegments("/signalr")
                                ? IdentityModel.AspNetCore.OAuth2Introspection.TokenRetrieval.FromAuthorizationHeader()
                                : IdentityModel.AspNetCore.OAuth2Introspection.TokenRetrieval.FromQueryString();

                            context.Token = func(context.Request);
                            return Task.CompletedTask;
                        }
                    };
                });

            return services;
        }


        public static IServiceCollection ConfigureHttpClients(this IServiceCollection services, IConfiguration configuration,
            IVaultClient vaultClient, string authorityUrl)
        {
            var clientOptions = vaultClient.Get(configuration["Edo:IdentityClient:Options"]).GetAwaiter().GetResult();
            var identityUri = new Uri(new Uri(authorityUrl), "/connect/token").ToString();
            var clientId = clientOptions["clientId"];
            var clientSecret = clientOptions["clientSecret"];

            services.AddAccessTokenManagement(options =>
            {
                options.Client.Clients.Add(HttpClientNames.AccessTokenClient, new ClientCredentialsTokenRequest
                {
                    Address = identityUri,
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                });
            });

            services.AddClientAccessTokenHttpClient(HttpClientNames.MapperApi, HttpClientNames.AccessTokenClient, client =>
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("Mapper:Endpoint"));
            });

            services.AddClientAccessTokenHttpClient(HttpClientNames.MapperManagement, HttpClientNames.AccessTokenClient, client =>
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("Mapper:Endpoint"));
            });

            services.AddClientAccessTokenHttpClient(HttpClientNames.VccApi, HttpClientNames.AccessTokenClient, client =>
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("VccService:Endpoint"));
            });

            services.AddClientAccessTokenHttpClient(HttpClientNames.DacManagementClient, HttpClientNames.AccessTokenClient, client =>
            {
                client.BaseAddress = new Uri(authorityUrl);
            });

            services.AddClientAccessTokenHttpClient(HttpClientNames.UsersManagementIdentityClient, HttpClientNames.AccessTokenClient, client =>
            {
                client.BaseAddress = new Uri(authorityUrl);
            });

            services.AddClientAccessTokenHttpClient(HttpClientNames.SupplierOptionsProvider, HttpClientNames.AccessTokenClient, client =>
            {
                client.BaseAddress = new Uri(authorityUrl);
            });

            services.AddClientAccessTokenHttpClient(HttpClientNames.CurrencyService, HttpClientNames.AccessTokenClient, client =>
            {
                client.BaseAddress = new Uri(configuration["CurrencyConverter:WebApiHost"]);
            })
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(GetDefaultRetryPolicy());

            services.AddHttpClient(HttpClientNames.Identity, client => client.BaseAddress = new Uri(authorityUrl));

            services.AddHttpClient(HttpClientNames.GoogleMaps, c => { c.BaseAddress = new Uri(configuration["Edo:Google:Endpoint"]); })
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(GetDefaultRetryPolicy());

            services.AddHttpClient(SendGridMailSender.HttpClientName)
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(GetDefaultRetryPolicy());

            services.AddHttpClient(HttpClientNames.Payfort)
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(GetDefaultRetryPolicy());

            services.AddHttpClient(HttpClientNames.ConnectorsGrpc)
                .AddClientAccessTokenHandler(HttpClientNames.AccessTokenClient)
                .AddHttpClientRequestLogging(configuration, options =>
                {
                    options.SanitizingFunction = entry =>
                    {
                        if (entry.RequestHeaders is not null && entry.RequestHeaders.TryGetValue("Authorization", out _))
                            entry.RequestHeaders["Authorization"] = @"Bearer [hidden]";

                        return entry;
                    };
                });

            services.AddCodeFirstGrpcClient<IRatesGrpcService>(o =>
            {
                o.Address = new Uri(configuration["CurrencyConverter:GrpcHost"]);
            }).AddClientAccessTokenHandler(HttpClientNames.AccessTokenClient);

            services.AddHttpClient(HttpClientNames.Connectors, client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(ConnectorClientRequestTimeoutSeconds);
                })
                .SetHandlerLifetime(TimeSpan.FromMinutes(ConnectorClientHandlerLifeTimeMinutes))
                .AddPolicyHandler((sp, _) => GetConnectorRetryPolicy(sp))
                .AddClientAccessTokenHandler(HttpClientNames.AccessTokenClient)
                .UseHttpClientMetrics()
                .AddHttpClientRequestLogging(configuration, options =>
                {
                    options.SanitizingFunction = entry =>
                    {
                        if (entry.RequestHeaders is not null && entry.RequestHeaders.TryGetValue("Authorization", out _))
                            entry.RequestHeaders["Authorization"] = @"Bearer [hidden]";

                        return entry;
                    };
                });

            return services;
        }


        public static IServiceCollection ConfigureServiceOptions(this IServiceCollection services, IConfiguration configuration,
            VaultClient.VaultClient vaultClient)
        {
            #region mailing setting

            var mailSettings = vaultClient.Get(configuration["Edo:Email:Options"]).GetAwaiter().GetResult();
            var edoAgentAppFrontendUrl = mailSettings[configuration["Edo:Email:EdoAgentAppFrontendUrl"]];

            var sendGridApiKey = mailSettings[configuration["Edo:Email:ApiKey"]];
            var senderAddress = mailSettings[configuration["Edo:Email:SenderAddress"]];
            services.Configure<SenderOptions>(options =>
            {
                options.ApiKey = sendGridApiKey;
                options.BaseUrl = new Uri(edoAgentAppFrontendUrl);
                options.SenderAddress = new EmailAddress(senderAddress);
            });

            var edoManagementFrontendUrl = mailSettings[configuration["Edo:Email:EdoManagementFrontendUrl"]];
            services.Configure<AdminInvitationMailOptions>(options =>
            {
                options.FrontendBaseUrl = edoManagementFrontendUrl;
            });

            var reservationsOfficeBackupEmail = mailSettings[configuration["Edo:Email:ReservationsOfficeBackupEmail"]];
            services.Configure<PropertyOwnerMailingOptions>(options =>
            {
                options.ReservationsOfficeBackupEmail = reservationsOfficeBackupEmail;
            });
            #endregion

            #region tag processing options

            services.Configure<TagProcessingOptions>(configuration.GetSection("TagProcessing"));

            #endregion

            services.Configure<BookingStatusUpdateOptions>(configuration.GetSection("BookingStatusUpdate"));

            var databaseOptions = vaultClient.Get(configuration["Edo:Database:Options"]).GetAwaiter().GetResult();
            services.AddEntityFrameworkNpgsql().AddDbContextPool<EdoContext>(options =>
            {
                var host = databaseOptions["host"];
                var port = databaseOptions["port"];
                var password = databaseOptions["password"];
                var userId = databaseOptions["userId"];

                var connectionString = configuration.GetConnectionString("Edo");
                options.UseNpgsql(string.Format(connectionString, host, port, userId, password), builder =>
                {
                    builder.UseNetTopologySuite();
                    builder.EnableRetryOnFailure();
                });
                options.EnableSensitiveDataLogging(false);
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }, 16);

            services.Configure<CurrencyRateServiceOptions>(configuration.GetSection("CurrencyConverter"));
            services.Configure<SupplierConnectorOptions>(configuration.GetSection("SupplierConnectorOptions"));

            var googleOptions = vaultClient.Get(configuration["Edo:Google:Options"]).GetAwaiter().GetResult();
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

                    options.RequestCultureProviders.Insert(0, new RouteDataRequestCultureProvider { Options = options });
                });

            services.Configure<LocationServiceOptions>(o =>
            {
                o.IsGoogleGeoCoderDisabled = bool.TryParse(googleOptions["disabled"], out var disabled) && disabled;
            });

            services.Configure<PaymentLinkOptions>(configuration.GetSection("PaymentLinks"));

            var payfortOptions = vaultClient.Get(configuration["Edo:Payfort:Options"]).GetAwaiter().GetResult();
            var payfortUrlsOptions = vaultClient.Get(configuration["Edo:Payfort:Urls"]).GetAwaiter().GetResult();
            services.Configure<PayfortOptions>(options =>
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

            services.Configure<ContractKindCommissionOptions>(options =>
            {
                options.CreditCardPaymentsCommission = configuration.GetValue<decimal>("ContractKindCommission:CreditCardPayments");
            });

            

            services.Configure<BankDetails>(configuration.GetSection("BankDetails"));

            var amazonS3DocumentsOptions = vaultClient.Get(configuration["AmazonS3:Options"]).GetAwaiter().GetResult();
            var contractsS3FolderName = configuration["AmazonS3:ContractsS3FolderName"];
            var imagesS3FolderName = configuration["AmazonS3:ImagesS3FolderName"];

            services.AddAmazonS3Client(options =>
            {
                options.AccessKeyId = amazonS3DocumentsOptions["accessKeyId"];
                options.SecretKey = amazonS3DocumentsOptions["accessKey"];
                options.AmazonS3Config = new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.GetBySystemName(amazonS3DocumentsOptions["regionEndpoint"])
                };
            });

            services.Configure<ContractFileServiceOptions>(options =>
            {
                options.Bucket = amazonS3DocumentsOptions["bucket"];
                options.S3FolderName = contractsS3FolderName;
            });

            services.Configure<ImageFileServiceOptions>(options =>
            {
                options.Bucket = amazonS3DocumentsOptions["bucket"];
                options.S3FolderName = imagesS3FolderName;
            });

            var urlGenerationOptions = vaultClient.Get(configuration["UrlGeneration:Options"]).GetAwaiter().GetResult();
            services.Configure<UrlGenerationOptions>(options =>
            {
                options.ConfirmationPageUrl = urlGenerationOptions["confirmationPageUrl"];
                options.AesKey = Convert.FromBase64String(urlGenerationOptions["aesKey"]);
                options.AesIV = Convert.FromBase64String(urlGenerationOptions["aesIV"]);
            });

            services.Configure<PaymentProcessorOption>(configuration.GetSection("PaymentProcessor"));
            services.Configure<MarkupPolicyStorageOptions>(configuration.GetSection("MarkupPolicyStorageOptions"));
            services.Configure<DiscountStorageOptions>(configuration.GetSection("DiscountStorageOptions"));
            services.Configure<SearchLimits>(configuration.GetSection("SearchLimits"));

            #region Configure NGenius

            var nGeniusOptions = vaultClient.Get(configuration["Edo:NGenius"]).GetAwaiter().GetResult();
            services.Configure<NGeniusOptions>(options =>
            {
                options.ApiKey = nGeniusOptions["apiKey"];
                options.Host = nGeniusOptions["host"];
                options.Outlets = new Dictionary<Currencies, string>
                {
                    {Currencies.USD, nGeniusOptions["usd"]},
                    {Currencies.AED, nGeniusOptions["aed"]}
                };
            });

            services.AddHttpClient(HttpClientNames.NGenius, c => { c.BaseAddress = new Uri(nGeniusOptions["host"]); })
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(GetDefaultRetryPolicy());

            #endregion

            return services;
        }


        public static IServiceCollection AddServices(this IServiceCollection services, IHostEnvironment environment, IConfiguration configuration, VaultClient.VaultClient vaultClient)
        {
            services.AddScoped<IdempotentFunctionExecutor>();
            services.AddScoped<IdempotentBookingExecutor>();

            services.AddSingleton(NtsGeometryServices.Instance.CreateGeometryFactory(GeoConstants.SpatialReferenceId));
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(EnvironmentVariableHelper.Get("Redis:Endpoint", configuration)));
            services.AddSingleton<IDistributedLocker, RedisDistributedLocker>();

            services.AddTransient<IConnectorClient, ConnectorClient>();
            services.AddTransient<ICountryService, CountryService>();
            services.AddTransient<IGeoCoder, GoogleGeoCoder>();

            services.AddSingleton<IVersionService, VersionService>();

            services.AddTransient<ILocationService, LocationService>();
            services.AddTransient<IAgencyVerificationService, AgencyVerificationService>();

            services.AddTransient<Services.Agents.IAgentService, Services.Agents.AgentService>();
            services.AddTransient<IAgentRolesService, AgentRolesService>();
            services.AddTransient<IAgentRegistrationService, AgentRegistrationService>();
            services.AddTransient<IAccountPaymentService, AccountPaymentService>();
            services.AddTransient<IAgencyAccountService, AgencyAccountService>();
            services.AddTransient<ICompanyAccountService, CompanyAccountService>();
            services.AddTransient<IPaymentSettingsService, PaymentSettingsService>();
            services.AddTransient<IBookingOfflinePaymentService, BookingOfflinePaymentService>();
            services.AddTransient<IBookingCreditCardPaymentService, BookingCreditCardPaymentService>();
            services.AddTransient<IBookingAccountPaymentService, BookingAccountPaymentService>();
            services.AddTransient<IBookingPaymentCallbackService, BookingPaymentCallbackService>();

            services.AddScoped<IAgentContextService, HttpBasedAgentContextService>();
            services.AddScoped<IAgentContextInternal, HttpBasedAgentContextService>();
            services.AddHttpContextAccessor();
            services.AddSingleton<IDateTimeProvider, DefaultDateTimeProvider>();
            services.AddTransient<IBookingRecordManager, BookingRecordManager>();
            services.AddTransient<ITagProcessor, TagProcessor>();

            services.AddSingleton<IMailSender, SendGridMailSender>();
            services.AddSingleton<ITokenInfoAccessor, TokenInfoAccessor>();
            services.AddSingleton<IIdentityUserInfoService, IdentityUserInfoService>();
            services.AddTransient<IAccountBalanceAuditService, AccountBalanceAuditService>();
            services.AddTransient<ICreditCardAuditService, CreditCardAuditService>();
            services.AddTransient<IOfflinePaymentAuditService, OfflinePaymentAuditService>();

            services.AddTransient<IAccountManagementService, AccountManagementService>();
            services.AddTransient<IAdministratorService, AdministratorService>();
            services.AddTransient<IAdministratorRolesManagementService, AdministratorRolesManagementService>();
            services.AddTransient<IAdministratorManagementService, AdministratorManagementService>();
            services.AddTransient<IAgentRolesManagementService, AgentRolesManagementService>();
            services.AddScoped<IAdministratorContext, HttpBasedAdministratorContext>();
            services.AddScoped<IServiceAccountContext, HttpBasedServiceAccountContext>();

            services.AddTransient<IInvitationRecordService, InvitationRecordService>();
            services.AddTransient<IAgentInvitationRecordListService, AgentInvitationRecordListService>();
            services.AddTransient<IAgentInvitationAcceptService, AgentInvitationAcceptService>();
            services.AddTransient<IAdminInvitationAcceptService, AdminInvitationAcceptService>();
            services.AddTransient<IAgentInvitationCreateService, AgentInvitationCreateService>();
            services.AddTransient<IAdminInvitationCreateService, AdminInvitationCreateService>();

            services.AddTransient<IExternalAdminContext, ExternalAdminContext>();

            services.AddScoped<IManagementAuditService, ManagementAuditService>();

            services.AddScoped<IEntityLocker, EntityLocker>();
            services.AddTransient<IAccountPaymentProcessingService, AccountPaymentProcessingService>();

            services.AddTransient<IPayfortService, PayfortService>();
            services.AddTransient<ICreditCardsManagementService, CreditCardsManagementService>();
            services.AddTransient<IPayfortSignatureService, PayfortSignatureService>();

            services.AddTransient<IMarkupPolicyService, MarkupPolicyService>();
            services.AddTransient<IMarkupService, MarkupService>();

            services.AddTransient<IDisplayedMarkupFormulaService, DisplayedMarkupFormulaService>();
            services.AddTransient<IMarkupBonusMaterializationService, MarkupBonusMaterializationService>();
            services.AddTransient<IMarkupBonusDisplayService, MarkupBonusDisplayService>();

            services.AddSingleton<IMarkupPolicyTemplateService, MarkupPolicyTemplateService>();
            services.AddScoped<IAgentMarkupPolicyManager, AgentMarkupPolicyManager>();
            services.AddScoped<IChildAgencyMarkupPolicyManager, ChildAgencyMarkupPolicyManager>();
            services.AddTransient<IMarkupPolicyAuditService, MarkupPolicyAuditService>();
            services.AddTransient<IAdminMarkupPolicyNotifications, AdminMarkupPolicyNotifications>();
            services.AddScoped<IAdminMarkupPolicyManager, AdminMarkupPolicyManager>();
            services.AddScoped<ISupplierMarkupPolicyManager, SupplierMarkupPolicyManager>();

            services.AddScoped<ICurrencyRateService, CurrencyRateService>();
            services.AddScoped<ICurrencyConverterService, CurrencyConverterService>();

            services.AddTransient<ISupplierOrderService, SupplierOrderService>();

            services.AddSingleton<IJsonSerializer, NewtonsoftJsonSerializer>();
            services.AddTransient<IAgentSettingsManager, AgentSettingsManager>();
            services.AddTransient<IAgentStatusManagementService, AgentStatusManagementService>();

            services.AddTransient<IPaymentLinkService, PaymentLinkService>();
            services.AddTransient<IPaymentLinksProcessingService, PaymentLinksProcessingService>();
            services.AddTransient<IPaymentLinksStorage, PaymentLinksStorage>();
            services.AddTransient<IPaymentCallbackDispatcher, PaymentCallbackDispatcher>();
            services.AddTransient<IAgentRolesAssignmentService, AgentRolesAssignmentService>();
            services.AddTransient<IPermissionChecker, PermissionChecker>();

            services.AddTransient<IBookingNotificationService, BookingNotificationService>();
            services.AddTransient<IBookingDocumentsMailingService, BookingDocumentsMailingService>();
            services.AddTransient<IBookingReportsService, BookingReportsService>();

            services.AddTransient<IPaymentHistoryService, PaymentHistoryService>();
            services.AddTransient<IBookingDocumentsService, BookingDocumentsService>();
            services.AddTransient<IBookingAuditLogService, BookingAuditLogService>();
            services.AddSingleton<ISupplierConnectorManager, SupplierConnectorManager>();
            services.AddTransient<IWideAvailabilitySearchService, WideAvailabilitySearchService>();
            services.AddTransient<IWideAvailabilityPriceProcessor, WideAvailabilityPriceProcessor>();
            services.AddTransient<IWideAvailabilityAccommodationsStorage, WideAvailabilityAccommodationsStorage>();

            services.AddTransient<IRoomSelectionService, RoomSelectionService>();
            services.AddTransient<IRoomSelectionPriceProcessor, RoomSelectionPriceProcessor>();

            services.AddTransient<IBookingEvaluationService, BookingEvaluationService>();
            services.AddTransient<IBookingEvaluationPriceProcessor, BookingEvaluationPriceProcessor>();

            services.AddTransient<ISupplierBookingManagementService, SupplierBookingManagementService>();
            services.AddTransient<IFinancialAccountBookingFlow, FinancialAccountBookingFlow>();
            services.AddTransient<IBankCreditCardBookingFlow, BankCreditCardBookingFlow>();
            services.AddTransient<IOfflinePaymentBookingFlow, OfflinePaymentBookingFlow>();
            services.AddTransient<IBookingInfoService, BookingInfoService>();
            services.AddTransient<IBookingRequestExecutor, BookingRequestExecutor>();
            services.AddTransient<IBookingRequestStorage, BookingRequestStorage>();
            services.AddTransient<IBookingResponseProcessor, BookingResponseProcessor>();

            services.AddTransient<IBookingRecordsUpdater, BookingRecordsUpdater>();
            services.AddTransient<IBookingRegistrationService, BookingRegistrationService>();
            services.AddTransient<IBookingChangeLogService, BookingChangeLogService>();

            services.AddTransient<IBookingMoneyReturnService, BookingMoneyReturnService>();
            services.AddTransient<IBookingsProcessingService, BookingsProcessingService>();
            services.AddTransient<IDeadlineService, DeadlineService>();
            services.AddTransient<IAppliedBookingMarkupRecordsManager, AppliedBookingMarkupRecordsManager>();

            services.AddTransient<IAgentBookingDocumentsService, AgentBookingDocumentsService>();

            services.AddSingleton<IAuthorizationPolicyProvider, CustomAuthorizationPolicyProvider>();
            services.AddTransient<IAuthorizationHandler, InAgencyPermissionAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, MinAgencyVerificationStateAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, AdministratorPermissionsAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, AgentRequiredAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, ServiceAccountRequiredAuthorizationHandler>();

            services.AddTransient<ICreditCardPaymentProcessingService, CreditCardPaymentProcessingService>();
            services.AddTransient<ICreditCardMoneyAuthorizationService, CreditCardMoneyAuthorizationService>();
            services.AddTransient<ICreditCardMoneyCaptureService, CreditCardMoneyCaptureService>();
            services.AddTransient<ICreditCardMoneyRefundService, CreditCardMoneyRefundService>();
            services.AddTransient<IPayfortResponseParser, PayfortResponseParser>();

            services.AddTransient<ICompanyService, CompanyService>();
            services.AddTransient<MailSenderWithCompanyInfo>();

            // Default behaviour allows not authenticated requests to be checked by authorization policies.
            // Special wrapper returns Forbid result for them.
            // More information: https://github.com/dotnet/aspnetcore/issues/4656
            services.AddTransient<IPolicyEvaluator, ForbidUnauthenticatedPolicyEvaluator>();
            // Default policy evaluator needs to be registered as dependency of ForbidUnauthenticatedPolicyEvaluator.
            services.AddTransient<PolicyEvaluator>();

            services.AddTransient<NetstormingResponseService>();
            services.AddTransient<WebhookResponseService>();

            services.AddNameNormalizationServices();

            services.AddTransient<IMultiProviderAvailabilityStorage, MultiProviderAvailabilityStorage>();
            services.AddTransient<IWideAvailabilitySearchStateStorage, WideAvailabilitySearchStateStorage>();
            services.AddTransient<IRoomSelectionStorage, RoomSelectionStorage>();

            services.AddMongoDbStorage(environment, configuration, vaultClient);
            services.AddTransient<IWideAvailabilityStorage, MongoDbWideAvailabilityStorage>();

            services.AddTransient<IBookingEvaluationStorage, BookingEvaluationStorage>();

            services.AddTransient<IRootAgencySystemSettingsService, RootAgencySystemSettingsService>();

            services.AddTransient<IPriceProcessor, PriceProcessor>();

            services.AddTransient<IInvoiceService, InvoiceService>();
            services.AddTransient<IReceiptService, ReceiptService>();
            services.AddTransient<IPaymentDocumentsStorage, PaymentDocumentsStorage>();
            services.AddTransient<IPaymentLinkNotificationService, PaymentLinkNotificationService>();

            services.AddTransient<AdministratorServices.IAgentService, AdministratorServices.AgentService>();
            services.AddTransient<IAgentSystemSettingsService, AgentSystemSettingsService>();
            services.AddTransient<IAgencySystemSettingsService, AgencySystemSettingsService>();

            services.AddTransient<IAgentSystemSettingsManagementService, AgentSystemSettingsManagementService>();
            services.AddTransient<IAgencySystemSettingsManagementService, AgencySystemSettingsManagementService>();

            services.AddTransient<IBookingService, BookingService>();
            services.AddTransient<IAccommodationBookingSettingsService, AccommodationBookingSettingsService>();

            services.AddTransient<IContractFileManagementService, ContractFileManagementService>();
            services.AddTransient<IContractFileService, ContractFileService>();
            services.AddTransient<IImageFileService, ImageFileService>();

            services.AddTransient<IAnalyticsService, ElasticAnalyticsService>();
            services.AddTransient<IBookingAnalyticsService, BookingAnalyticsService>();
            services.AddTransient<IAgentMovementService, AgentMovementService>();

            services.AddTransient<IAgentBookingManagementService, AgentBookingManagementService>();
            services.AddTransient<IAdministratorBookingManagementService, AdministratorBookingManagementService>();
            services.AddTransient<IBookingStatusRefreshService, BookingStatusRefreshService>();

            services.AddTransient<IApiClientManagementService, ApiClientManagementService>();
            services.AddTransient<IAccommodationMapperClient, AccommodationMapperClient>();
            services.AddTransient<IMapperManagementClient, MapperManagementClient>();
            services.AddTransient<IAvailabilitySearchAreaService, AvailabilitySearchAreaService>();

            services.AddTransient<IAdminAgencyManagementService, AdminAgencyManagementService>();
            services.AddTransient<IAgencyManagementService, AgencyManagementService>();
            services.AddTransient<IChildAgencyService, ChildAgencyService>();
            services.AddTransient<IAgencyService, AgencyService>();

            services.AddTransient<IAgencyRemovalService, AgencyRemovalService>();
            services.AddTransient<IAgentRemovalService, AgentRemovalService>();

            services.AddTransient<IApiClientService, ApiClientService>();
            services.AddTransient<IReportService, ReportService>();

            services.AddTransient<IAdministratorRolesAssignmentService, AdministratorRolesAssignmentService>();

            services.AddTransient<IConverter<AgencyWiseRecordData, AgencyWiseReportRow>, AgencyWiseRecordDataConverter>();
            services.AddTransient<IConverter<PayableToSupplierRecordData, PayableToSupplierReportRow>, PayableToSupplierRecordDataConverter>();
            services.AddTransient<IConverter<FullBookingsReportData, FullBookingsReportRow>, FullBookingsReportDataConverter>();
            services.AddTransient<IConverter<FinalizedBookingsReportData, FinalizedBookingsReportRow>, FinalizedBookingsReportDataConverter>();
            services.AddTransient<IConverter<HotelWiseData, HotelWiseRow>, HotelWiseBookingReportDataConverter>();
            services.AddTransient<IRecordManager<AgencyWiseRecordData>, AgencyWiseRecordManager>();
            services.AddTransient<IRecordManager<PayableToSupplierRecordData>, PayableToSupplierRecordsManager>();
            services.AddTransient<IRecordManager<FullBookingsReportData>, FullBookingsRecordManager>();
            services.AddTransient<IRecordManager<FinalizedBookingsReportData>, FinalizedBookingsRecordManager>();
            services.AddTransient<IConverter<AgencyWiseRecordData, AgencyWiseReportRow>, AgencyWiseRecordDataConverter>();
            services.AddTransient<IConverter<PayableToSupplierRecordData, PayableToSupplierReportRow>, PayableToSupplierRecordDataConverter>();
            services.AddTransient<IConverter<FullBookingsReportData, FullBookingsReportRow>, FullBookingsReportDataConverter>();
            services.AddTransient<IConverter<PendingSupplierReferenceData, PendingSupplierReferenceRow>, PendingSupplierReferenceProjectionConverter>();
            services.AddTransient<IConverter<ConfirmedBookingsData, ConfirmedBookingsRow>, ConfirmedBookingsConverter>();
            services.AddTransient<IConverter<VccBookingData, VccBookingRow>, VccBookingDataConverter>();
            services.AddTransient<IConverter<AgentWiseReportData, AgentWiseReportRow>, AgentWiseRecordDataConverter>();
            services.AddTransient<IRecordManager<AgencyWiseRecordData>, AgencyWiseRecordManager>();
            services.AddTransient<IRecordManager<PayableToSupplierRecordData>, PayableToSupplierRecordsManager>();
            services.AddTransient<IRecordManager<FullBookingsReportData>, FullBookingsRecordManager>();
            services.AddTransient<IRecordManager<PendingSupplierReferenceData>, PendingSupplierReferenceRecordManager>();
            services.AddTransient<IRecordManager<ConfirmedBookingsData>, ConfirmedBookingsRecordManager>();
            services.AddTransient<IRecordManager<AgencyProductivity>, AgenciesProductivityRecordManager>();
            services.AddTransient<IRecordManager<HotelWiseData>, HotelWiseRecordManager>();
            services.AddTransient<IRecordManager<CancellationDeadlineData>, CancellationDeadlineReportManager>();
            services.AddTransient<IRecordManager<ThirdPartySupplierData>, ThirdPartySuppliersReportManager>();
            services.AddTransient<IRecordManager<VccBookingData>, VccBookingRecordManager>();
            services.AddTransient<IRecordManager<AgentWiseReportData>, AgentWiseRecordManager>();
            services.AddTransient<IRecordManager<HotelProductivityData>, HotelProductivityRecordManager>();
            services.AddTransient<IRecordManager<CancelledBookingsReportData>, CancelledBookingsReportRecordManager>();
            services.AddTransient<IFixHtIdService, FixHtIdService>();

            services.AddTransient<IBookingConfirmationService, BookingConfirmationService>();
            services.AddTransient<IPropertyOwnerConfirmationUrlGenerator, PropertyOwnerConfirmationUrlGenerator>();
            services.AddTransient<INGeniusClient, NGeniusClient>();
            services.AddTransient<INGeniusPaymentService, NGeniusPaymentService>();
            services.AddTransient<NGeniusWebhookProcessingService>();
            services.AddTransient<INGeniusRefundService, NGeniusRefundService>();
            services.AddTransient<ICreditCardPaymentManagementService, CreditCardPaymentManagementService>();
            services.AddTransient<IBalanceNotificationsManagementService, BalanceNotificationsManagementService>();
            services.AddTransient<IBalanceManagementNotificationsService, BalanceManagementNotificationsService>();
            services.AddHostedService<MarkupPolicyStorageUpdater>();
            services.AddSingleton<IMarkupPolicyStorage, MarkupPolicyStorage>();
            services.AddTransient<ILocalityInfoService, LocalityInfoService>();
            services.AddTransient<IDirectApiClientManagementService, DirectApiClientManagementService>();
            services.AddTransient<IAvailabilityRequestStorage, AvailabilityRequestStorage>();
            services.AddTransient<IMarketManagementService, MarketManagementService>();
            services.AddTransient<IMarketManagementStorage, MarketManagementStorage>();
            services.AddTransient<IAgencySupplierManagementService, AgencySupplierManagementService>();
            services.AddTransient<IAgentSupplierManagementService, AgentSupplierManagementService>();
            services.AddTransient<IMessageBus, MessageBus>();

            var suppliersEndpoint = configuration.GetValue<string>("Suppliers:Endpoint");
            services.AddSupplierOptionsProvider(options =>
            {
                options.IdentityClientName = HttpClientNames.AccessTokenClient;
                options.BaseEndpoint = suppliersEndpoint;
                options.StorageTimeout = TimeSpan.FromSeconds(60);
                options.UpdaterInterval = TimeSpan.FromSeconds(60);
            });

            services.AddCreditCardProvider(configuration, vaultClient);

            //TODO: move to Consul when it will be ready
            services.AddCurrencyConversionFactory(new List<BufferPair>
            {
                new()
                {
                    BufferValue = decimal.Zero,
                    SourceCurrency = Currencies.AED,
                    TargetCurrency = Currencies.USD
                },
                new()
                {
                    BufferValue = decimal.Zero,
                    SourceCurrency = Currencies.USD,
                    TargetCurrency = Currencies.AED
                },
                new()
                {
                    BufferValue = 0.02m,
                    SourceCurrency = Currencies.USD,
                    TargetCurrency = Currencies.OMR
                },
                new()
                {
                    BufferValue = 0.02m,
                    SourceCurrency = Currencies.OMR,
                    TargetCurrency = Currencies.USD
                }
            });

            var natsEndpoints = configuration.GetValue<string>("Nats:Endpoints").Split(";");
            services.AddNatsClient(options =>
            {
                options.Servers = natsEndpoints;
                options.MaxReconnect = NATS.Client.Options.ReconnectForever;
            }, ServiceLifetime.Singleton);

            return services;
        }


        public static IServiceCollection AddUserEventLogging(this IServiceCollection services, IConfiguration configuration,
            VaultClient.VaultClient vaultClient)
        {
            var elasticOptions = vaultClient.Get(configuration["UserEvents:ElasticSearch"]).GetAwaiter().GetResult();
            return services.AddSingleton<IElasticLowLevelClient>(provider =>
            {
                var settings = new ConnectionConfiguration(new Uri(elasticOptions["endpoint"]))
                    .BasicAuthentication(elasticOptions["username"], elasticOptions["password"])
                    .ServerCertificateValidationCallback((o, certificate, chain, errors) => true)
                    .ClientCertificate(new X509Certificate2(Convert.FromBase64String(elasticOptions["certificate"])));
                var client = new ElasticLowLevelClient(settings);

                return client;
            });
        }


        private static IAsyncPolicy<HttpResponseMessage> GetDefaultRetryPolicy()
        {
            var jitter = new Random();

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.ServiceUnavailable)
                .WaitAndRetryAsync(3, attempt
                    => TimeSpan.FromSeconds(Math.Pow(1.5, attempt)) + TimeSpan.FromMilliseconds(jitter.Next(0, 100)));
        }


        private static IAsyncPolicy<HttpResponseMessage> GetConnectorRetryPolicy(IServiceProvider serviceProvider)
        {
            var jitter = new Random();

            return Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(msg => ConnectorRetryStatuses.Contains(msg.StatusCode))
                .WaitAndRetryAsync(3,
                    attempt => TimeSpan.FromMilliseconds(attempt * 500 + jitter.Next(0, 100)),
                    (handler, timeSpan, retryAttempt, _) =>
                    {
                        var errorMessage = handler.Exception?.Message
                            ?? $"{handler.Result.StatusCode} {handler.Result.Content.ReadAsStringAsync().Result}";

                        var logger = serviceProvider.GetRequiredService<ILogger<HttpClient>>();
                        logger.LogConnectorClientDelay(timeSpan.TotalMilliseconds, errorMessage, retryAttempt);
                    }
                );
        }


        private static IServiceCollection AddCreditCardProvider(this IServiceCollection services, IConfiguration configuration, VaultClient.VaultClient vaultClient)
        {
            var creditCardProvider = configuration.GetValue<CreditCardProviderTypes>("CreditCardProvider");
            if (creditCardProvider == CreditCardProviderTypes.Vcc)
            {
                services.AddVccService(options =>
                {
                    options.VccEndpoint = configuration["VccService:Endpoint"];
                    options.IdentityClientName = HttpClientNames.AccessTokenClient;
                });
                services.AddTransient<ICreditCardProvider, VirtualCreditCardProvider>();
            }
            else if (creditCardProvider == CreditCardProviderTypes.Actual)
            {
                services.AddTransient<ICreditCardProvider, ActualCreditCardProvider>();
                var actualCreditCardOptions = vaultClient.Get("edo/actual-credit-cards/AED").GetAwaiter().GetResult();
                services.Configure<ActualCreditCardOptions>(o =>
                {
                    // Only AED card is supported for now
                    var card = new HappyTravel.Edo.CreditCards.Models.CreditCardInfo(Number: actualCreditCardOptions["number"],
                        ExpiryDate: DateTime.Parse(actualCreditCardOptions["expiry"], CultureInfo.InvariantCulture),
                        HolderName: actualCreditCardOptions["holder"],
                        SecurityCode: actualCreditCardOptions["code"]);

                    o.Cards = new Dictionary<Currencies, HappyTravel.Edo.CreditCards.Models.CreditCardInfo>()
                    {
                        {Currencies.AED, card}
                    };
                });
            }

            return services;
        }


        private static readonly HashSet<HttpStatusCode> ConnectorRetryStatuses = new()
        {
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.Unauthorized
        };


        private const int ConnectorClientHandlerLifeTimeMinutes = 5;
        private const int ConnectorClientRequestTimeoutSeconds = 130;
    }
}