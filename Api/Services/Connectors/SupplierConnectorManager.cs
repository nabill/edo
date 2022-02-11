using System;
using System.Collections.Concurrent;
using System.Net.Http;
using Grpc.Net.Client;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Infrastructure.SupplierConnectors;
using HappyTravel.Edo.Api.Services.CurrencyConversion;
using HappyTravel.EdoContracts.Grpc.Services;
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

        
        public ISupplierConnector Get(int key)
        {
            return _supplierConnectorOptions.CurrentValue.ClientType switch
            {
                ClientTypes.WebApi => GetRestApiConnector(key),
                ClientTypes.Grpc => GetGrpcConnector(key),
                _ => throw new NotSupportedException($"{_supplierConnectorOptions.CurrentValue.ClientType} not supported")
            };
        }


        private ISupplierConnector GetRestApiConnector(int key)
        {
            var supplier = _supplierStorage.GetById(key);
            var client = _serviceProvider.GetRequiredService<IConnectorClient>();
            var logger = _serviceProvider.GetRequiredService<ILogger<SupplierConnector>>();
            
            return new SupplierConnector(
                supplierName: supplier.Name,
                connectorClient: client,
                baseUrl: supplier.ConnectorUrl,
                logger: logger);
        }


        private ISupplierConnector GetGrpcConnector(int key)
        {
            var supplier = _supplierStorage.GetById(key);
            var client = GetGrpcClient(supplier);
            var logger = _serviceProvider.GetRequiredService<ILogger<SupplierGrpcConnector>>();
            
            return new SupplierGrpcConnector(supplierName: supplier.Name,
                connectorClient: client,
                logger: logger);
        }


        private IConnectorGrpcService GetGrpcClient(Supplier supplier)
        {
            if (string.IsNullOrEmpty(supplier.ConnectorGrpcEndpoint))
                throw new Exception($"Supplier {supplier.Name} gRPC endpoint is null or empty");

            if (_grcClients.TryGetValue(supplier.Id, out var client))
                return client;

            var channel = GrpcChannel.ForAddress(supplier.ConnectorGrpcEndpoint, new GrpcChannelOptions
            {
                HttpClient = _httpClientFactory.CreateClient(HttpClientNames.ConnectorsGrpc)
            });
            client = channel.CreateGrpcService<IConnectorGrpcService>();
            _grcClients.AddOrUpdate(supplier.Id, client, (_, _) => client);
            
            return client;
        }


        private readonly ConcurrentDictionary<int, IConnectorGrpcService> _grcClients = new();


        private readonly IServiceProvider _serviceProvider;
        private readonly ISupplierOptionsStorage _supplierStorage;
        private readonly IOptionsMonitor<SupplierConnectorOptions> _supplierConnectorOptions;
        private readonly IHttpClientFactory _httpClientFactory;
    }
}