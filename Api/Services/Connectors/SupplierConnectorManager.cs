using System;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Infrastructure.SupplierConnectors;
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
            var supplier = _supplierStorage.GetById(key);
            var connectorClient = _serviceProvider.GetRequiredService<IConnectorClient>();
            var logger = _serviceProvider.GetRequiredService<ILogger<SupplierConnector>>();

            return new SupplierConnector(
                supplierName: supplier.Name,
                connectorClient: connectorClient,
                baseUrl: supplier.ConnectorUrl,
                gRpcEndpoint: supplier.ConnectorGrpcEndpoint,
                clientType: _supplierConnectorOptions.CurrentValue.ClientType,
                logger: logger);
        }

        
        private readonly IServiceProvider _serviceProvider;
        private readonly ISupplierOptionsStorage _supplierStorage;
        private readonly IOptionsMonitor<SupplierConnectorOptions> _supplierConnectorOptions;
    }
}