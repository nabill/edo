using System;
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
        _connection.SubscribeAsync(MessageBusTopics.SupplierUpdated, (_, m) =>
        {
            _logger.LogSupplierUpdateMessageReceived();

            try
            {
                var supplier = JsonSerializer.Deserialize<SlimSupplier>(m.Message.Data);
                if (supplier is null)
                {
                    _logger.LogSupplierUpdateMessageDeserializationFailed();
                    return;
                }

                _clientsStorage.Update(supplier);
            }
            catch (Exception ex)
            {
                _logger.LogGrpcSupplierClientUpdateFailed(ex);
            }
        });
        
        return Task.CompletedTask;
    }


    private readonly IGrpcClientsStorage _clientsStorage;
    private readonly IConnection _connection;
    private readonly ILogger<GrpcClientsStorageUpdater> _logger;
}