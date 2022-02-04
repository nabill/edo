using System;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Infrastructure.SupplierConnectors;
using HappyTravel.Edo.Api.Services.CurrencyConversion;
using HappyTravel.SupplierOptionsProvider;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Connectors
{
    public class SupplierConnectorManager : ISupplierConnectorManager
    {
        public SupplierConnectorManager(IServiceProvider serviceProvider, ISupplierOptionsStorage supplierStorage, IOptionsMonitor<SupplierConnectorOptions> supplierConnectorOptions)
        {
            _serviceProvider = serviceProvider;
            _supplierStorage = supplierStorage;
            _supplierConnectorOptions = supplierConnectorOptions;
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
            return new GrpcSupplierConnector();
        }

        
        private readonly IServiceProvider _serviceProvider;
        private readonly ISupplierOptionsStorage _supplierStorage;
        private readonly IOptionsMonitor<SupplierConnectorOptions> _supplierConnectorOptions;
    }
}