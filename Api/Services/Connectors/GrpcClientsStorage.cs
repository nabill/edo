using System;
using System.Collections.Concurrent;
using System.Net.Http;
using Grpc.Net.Client;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.EdoContracts.Grpc.Services;
using HappyTravel.SupplierOptionsClient.Models;
using Microsoft.Extensions.Logging;
using ProtoBuf.Grpc.Client;

namespace HappyTravel.Edo.Api.Services.Connectors;

public class GrpcClientsStorage : IGrpcClientsStorage
{
    public GrpcClientsStorage(IHttpClientFactory httpClientFactory, ILogger<GrpcClientsStorage> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }


    /// <summary>
    ///     Returns existed or new IConnectorGrpcService for supplier
    /// </summary>
    public IConnectorGrpcService Get(SlimSupplier supplier)
    {
        return _grcClients.TryGetValue(supplier.Code, out var client) 
            ? client 
            : AddOrUpdate(supplier);
    }

    
    /// <summary>
    ///     Updates existed IConnectionGrpcService
    /// </summary>
    /// <param name="supplier"></param>
    public void Update(SlimSupplier supplier)
    {
        if (!_grcClients.TryGetValue(supplier.Code, out _))
        {
            _logger.LogGrpcSupplierClientNotFound(supplier.Code);
            return;
        }
        
        AddOrUpdate(supplier);
    }


    private IConnectorGrpcService AddOrUpdate(SlimSupplier supplier)
    {
        if (string.IsNullOrEmpty(supplier.ConnectorGrpcEndpoint))
            throw new Exception($"Supplier {supplier.Name} gRPC endpoint is null or empty");
        
        var channel = GrpcChannel.ForAddress(supplier.ConnectorGrpcEndpoint, new GrpcChannelOptions
        {
            HttpClient = _httpClientFactory.CreateClient(HttpClientNames.ConnectorsGrpc),
            MaxReceiveMessageSize = null // Unlimited size (default 4mb)
        });
        
        var client = channel.CreateGrpcService<IConnectorGrpcService>();
        _grcClients.AddOrUpdate(supplier.Code, client, (_, _) => client);
        _logger.LogGrpcSupplierClientUpdated(supplier.Code);

        return client;
    }


    private readonly ConcurrentDictionary<string, IConnectorGrpcService> _grcClients = new();
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GrpcClientsStorage> _logger;
}