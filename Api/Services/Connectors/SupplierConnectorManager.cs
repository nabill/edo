using System;
using System.Collections.Concurrent;
using System.Net.Http;
using CSharpFunctionalExtensions;
using Grpc.Net.Client;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Infrastructure.SupplierConnectors;
using HappyTravel.Edo.Api.Services.CurrencyConversion;
using HappyTravel.EdoContracts.Grpc.Services;
using HappyTravel.SupplierOptionsClient.Models;
using HappyTravel.SupplierOptionsProvider;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProtoBuf.Grpc.Client;

namespace HappyTravel.Edo.Api.Services.Connectors
{
    public class SupplierConnectorManager : ISupplierConnectorManager
    {
        public SupplierConnectorManager(IServiceProvider serviceProvider, ISupplierOptionsStorage supplierStorage, IOptionsMonitor<SupplierConnectorOptions> supplierConnectorOptions, IHttpClientFactory httpClientFactory)
        {
            _serviceProvider = serviceProvider;
            _supplierStorage = supplierStorage;
            _supplierConnectorOptions = supplierConnectorOptions;
            _httpClientFactory = httpClientFactory;
        }

        
        public ISupplierConnector Get(string supplierCode, ClientTypes? clientType = null)
        {
            var (_, isFailure, supplier, error) = _supplierStorage.Get(supplierCode);
            if (isFailure)
                throw new Exception($"Cannot get supplier `{supplierCode}` with error: {error}");

            clientType ??= _supplierConnectorOptions.CurrentValue.ClientType;
            
            return clientType switch
            {
                ClientTypes.WebApi => GetRestApiConnector(supplier),
                ClientTypes.Grpc => GetGrpcConnector(supplier),
                _ => throw new NotSupportedException($"{_supplierConnectorOptions.CurrentValue.ClientType} not supported")
            };
        }
        

        private ISupplierConnector GetRestApiConnector(SlimSupplier supplier)
        {
            var client = _serviceProvider.GetRequiredService<IConnectorClient>();
            var logger = _serviceProvider.GetRequiredService<ILogger<SupplierConnector>>();
            
            return new SupplierConnector(
                supplierName: supplier.Name,
                connectorClient: client,
                baseUrl: supplier.ConnectorUrl,
                logger: logger,
                customHeaders: supplier.CustomHeaders);
        }


        private ISupplierConnector GetGrpcConnector(SlimSupplier supplier)
        {
            var client = GetGrpcClient(supplier);
            var logger = _serviceProvider.GetRequiredService<ILogger<SupplierGrpcConnector>>();
            
            return new SupplierGrpcConnector(supplierName: supplier.Name,
                connectorClient: client,
                logger: logger,
                customHeaders: supplier.CustomHeaders);
        }


        private IConnectorGrpcService GetGrpcClient(SlimSupplier supplier)
        {
            if (string.IsNullOrEmpty(supplier.ConnectorGrpcEndpoint))
                throw new Exception($"Supplier {supplier.Name} gRPC endpoint is null or empty");

            if (_grcClients.TryGetValue(supplier.Code, out var client))
                return client;

            var channel = GrpcChannel.ForAddress(supplier.ConnectorGrpcEndpoint, new GrpcChannelOptions
            {
                HttpClient = _httpClientFactory.CreateClient(HttpClientNames.ConnectorsGrpc)
            });
            client = channel.CreateGrpcService<IConnectorGrpcService>();
            _grcClients.AddOrUpdate(supplier.Code, client, (_, _) => client);
            
            return client;
        }


        private readonly ConcurrentDictionary<string, IConnectorGrpcService> _grcClients = new();


        private readonly IServiceProvider _serviceProvider;
        private readonly ISupplierOptionsStorage _supplierStorage;
        private readonly IOptionsMonitor<SupplierConnectorOptions> _supplierConnectorOptions;
        private readonly IHttpClientFactory _httpClientFactory;
    }
}