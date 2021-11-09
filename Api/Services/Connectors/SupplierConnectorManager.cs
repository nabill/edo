using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Infrastructure.SupplierConnectors;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.SuppliersCatalog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Connectors
{
    public class SupplierConnectorManager : ISupplierConnectorManager
    {
        public SupplierConnectorManager(IOptions<SupplierOptions> options,
            IConnectorClient connectorClient,
            IServiceProvider serviceProvider)
        {
            _options = options.Value;
            _suppliers = new Dictionary<Suppliers, ISupplierConnector>
            {
                // TODO: Add other suppliers.
                {
                    Suppliers.Netstorming,
                    new SupplierConnector(Suppliers.Netstorming, connectorClient, _options.Netstorming, serviceProvider.GetRequiredService<ILogger<SupplierConnector>>())
                },
                {
                    Suppliers.Illusions,
                    new SupplierConnector(Suppliers.Illusions, connectorClient, _options.Illusions, serviceProvider.GetRequiredService<ILogger<SupplierConnector>>())
                },
                {
                    Suppliers.Etg,
                    new SupplierConnector(Suppliers.Etg, connectorClient, _options.Etg, serviceProvider.GetRequiredService<ILogger<SupplierConnector>>())
                },
                {
                    Suppliers.DirectContracts,
                    new SupplierConnector(Suppliers.DirectContracts, connectorClient, _options.DirectContracts, serviceProvider.GetRequiredService<ILogger<SupplierConnector>>())
                },
                {
                    Suppliers.Rakuten,
                    new SupplierConnector(Suppliers.Rakuten, connectorClient, _options.Rakuten, serviceProvider.GetRequiredService<ILogger<SupplierConnector>>())
                },
                {
                    Suppliers.Columbus,
                    new SupplierConnector(Suppliers.Columbus, connectorClient, _options.Columbus, serviceProvider.GetRequiredService<ILogger<SupplierConnector>>())
                },
                {
                    Suppliers.TravelgateXTest,
                    new SupplierConnector(Suppliers.TravelgateXTest, connectorClient, _options.TravelgateXTest, serviceProvider.GetRequiredService<ILogger<SupplierConnector>>())
                },
                {
                    Suppliers.Darina,
                    new SupplierConnector(Suppliers.Darina, connectorClient, _options.Darina, serviceProvider.GetRequiredService<ILogger<SupplierConnector>>())
                },
                {
                    Suppliers.Jumeirah,
                    new SupplierConnector(Suppliers.Jumeirah, connectorClient, _options.Jumeirah, serviceProvider.GetRequiredService<ILogger<SupplierConnector>>())
                },
                {
                    Suppliers.Paximum,
                    new SupplierConnector(Suppliers.Paximum, connectorClient, _options.Paximum, serviceProvider.GetRequiredService<ILogger<SupplierConnector>>())
                },
                {
                    Suppliers.Yalago,
                    new SupplierConnector(Suppliers.Yalago, connectorClient, _options.Yalago, serviceProvider.GetRequiredService<ILogger<SupplierConnector>>())
                },
                {
                    Suppliers.HotelBeds,
                    new SupplierConnector(Suppliers.HotelBeds, connectorClient, _options.HotelBeds, serviceProvider.GetRequiredService<ILogger<SupplierConnector>>())
                },
                {
                    Suppliers.Roibos,
                    new SupplierConnector(Suppliers.Roibos, connectorClient, _options.Roibos, serviceProvider.GetRequiredService<ILogger<SupplierConnector>>())
                },
                {
                    Suppliers.Bronevik,
                    new SupplierConnector(Suppliers.Bronevik, connectorClient, _options.Bronevik, serviceProvider.GetRequiredService<ILogger<SupplierConnector>>())
                },
                {
                    Suppliers.HotelbookPro,
                    new SupplierConnector(Suppliers.HotelbookPro, connectorClient, _options.HotelbookPro, serviceProvider.GetRequiredService<ILogger<SupplierConnector>>())
                },
                {
                    Suppliers.BookMe,
                    new SupplierConnector(Suppliers.BookMe, connectorClient, _options.BookMe, serviceProvider.GetRequiredService<ILogger<SupplierConnector>>())
                }
            };
        }

        public ISupplierConnector Get(Suppliers key) => _suppliers[key];
        
        private readonly Dictionary<Suppliers, ISupplierConnector> _suppliers;
        private readonly SupplierOptions _options;
    }
}