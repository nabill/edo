using System;
using System.Collections.Generic;
using System.Net.Http;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Infrastructure.SupplierConnectors;
using HappyTravel.SupplierOptionsProvider;
using HappyTravel.SuppliersCatalog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Connectors
{
    public class SupplierConnectorManager : ISupplierConnectorManager
    {
        public SupplierConnectorManager(IServiceProvider serviceProvider, ISupplierOptionsStorage supplierStorage)
        {
            _serviceProvider = serviceProvider;
            _supplierStorage = supplierStorage;
        }


        public ISupplierConnector Get(Suppliers key)
        {
            var supplier = _supplierStorage.GetById((int) key);
            var connectorClient = _serviceProvider.GetRequiredService<IConnectorClient>();
            var logger = _serviceProvider.GetRequiredService<ILogger<SupplierConnector>>();

            return new SupplierConnector(
                supplier: (Suppliers)supplier.Id,
                connectorClient: connectorClient,
                baseUrl: supplier.ConnectorUrl,
                logger: logger);
        }

        
        private readonly IServiceProvider _serviceProvider;
        private readonly ISupplierOptionsStorage _supplierStorage;
    }
}