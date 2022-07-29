using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.SupplierOptionsClient.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NATS.Client;

namespace HappyTravel.Edo.Api.Services.Connectors;

public class GrpcClientsStorageUpdater : BackgroundService
{
    public GrpcClientsStorageUpdater(IGrpcClientsStorage clientsStorage, IConnection connection, ILogger<GrpcClientsStorageUpdater> logger)
    {
        _clientsStorage = clientsStorage;
        _connection = connection;
        _logger = logger;
    }


    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _connection.SubscribeAsync(MessageBusTopics.MarkupPolicyUpdated, (_, m) =>
        {
            _logger.LogSupplierUpdateMessageReceived();
            
            var supplier = JsonSerializer.Deserialize<SlimSupplier>(m.Message.Data);
            if (supplier is null)
                return;

            _clientsStorage.Update(supplier);
        });
        
        return Task.CompletedTask;
    }


    private readonly IGrpcClientsStorage _clientsStorage;
    private readonly IConnection _connection;
    private readonly ILogger<GrpcClientsStorageUpdater> _logger;
}