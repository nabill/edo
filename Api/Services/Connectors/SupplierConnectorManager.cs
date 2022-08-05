using System;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.SupplierConnectors;
using HappyTravel.Edo.Api.Services.CurrencyConversion;
using HappyTravel.SupplierOptionsClient.Models;
using HappyTravel.SupplierOptionsProvider;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Connectors
{
    public class SupplierConnectorManager : ISupplierConnectorManager
    {
        public SupplierConnectorManager(IServiceProvider serviceProvider, ISupplierOptionsStorage supplierStorage, IGrpcClientsStorage clientsStorage)
        {
            _serviceProvider = serviceProvider;
            _supplierStorage = supplierStorage;
            _clientsStorage = clientsStorage;
        }

        
        public ISupplierConnector Get(string supplierCode, ClientTypes? clientType = null)
        {
            var (_, isFailure, supplier, error) = _supplierStorage.Get(supplierCode);
            if (isFailure)
                throw new Exception($"Cannot get supplier `{supplierCode}` with error: {error}");

            clientType ??= supplier.CanUseGrpc
                ? ClientTypes.Grpc
                : ClientTypes.WebApi;
            
            return clientType switch
            {
                ClientTypes.WebApi => GetRestApiConnector(supplier),
                ClientTypes.Grpc => GetGrpcConnector(supplier),
                _ => throw new NotSupportedException($"Client type `{clientType}` not supported")
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
            var client = _clientsStorage.Get(supplier);
            var logger = _serviceProvider.GetRequiredService<ILogger<SupplierGrpcConnector>>();
            
            return new SupplierGrpcConnector(supplierName: supplier.Name,
                connectorClient: client,
                logger: logger,
                customHeaders: supplier.CustomHeaders);
        }

        
        private readonly IServiceProvider _serviceProvider;
        private readonly ISupplierOptionsStorage _supplierStorage;
        private readonly IGrpcClientsStorage _clientsStorage;
    }
}