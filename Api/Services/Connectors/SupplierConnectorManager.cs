using System;
using System.Collections.Generic;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Infrastructure.SupplierConnectors;
using HappyTravel.SuppliersCatalog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Connectors
{
    public class SupplierConnectorManager : ISupplierConnectorManager
    {
        public SupplierConnectorManager(IOptionsMonitor<SupplierOptions> options, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            options.OnChange(UpdateConnectorClients);
            
            UpdateConnectorClients(options.CurrentValue);
        }


        private void UpdateConnectorClients(SupplierOptions options)
        {
            var connectorClient = _serviceProvider.GetRequiredService<IConnectorClient>();
            var logger = _serviceProvider.GetRequiredService<ILogger<SupplierConnector>>();
            var suppliers = new Dictionary<Suppliers, ISupplierConnector>();
            
            foreach (var (supplier, endpoint) in options.Endpoints)
            {
                suppliers.Add(supplier, new SupplierConnector(
                    supplier: supplier, 
                    connectorClient: connectorClient, 
                    baseUrl: endpoint, 
                    logger: logger));
            }

            _suppliers = suppliers;
        }
        

        public ISupplierConnector Get(Suppliers key) => _suppliers[key];

        
        private Dictionary<Suppliers, ISupplierConnector> _suppliers = new();


        private readonly IServiceProvider _serviceProvider;
    }
}