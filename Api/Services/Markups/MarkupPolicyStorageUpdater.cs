using System.Threading;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NATS.Client;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class MarkupPolicyStorageUpdater : BackgroundService
    {
        public MarkupPolicyStorageUpdater(IServiceScopeFactory scopeFactory, IMarkupPolicyStorage storage, IConnection connection)
        {
            _scopeFactory = scopeFactory;
            _storage = storage;
            _connection = connection;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _connection.SubscribeAsync(NatsTopics.MarkupPolicyUpdated, (_, _) => UpdateStorage());
            UpdateStorage();
        }


        private void UpdateStorage()
        {
            Task.Run(async () =>
            {
                var scope = _scopeFactory.CreateScope();
                await using var context = scope.ServiceProvider.GetRequiredService<EdoContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<MarkupPolicyStorageUpdater>>();
                var markupPolicies = await context.MarkupPolicies
                    .ToListAsync();
            
                _storage.Set(markupPolicies);
                logger.LogMarkupPolicyStorageRefreshed(markupPolicies.Count);
            });
        }
        
        
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMarkupPolicyStorage _storage;
        private readonly IConnection _connection;
    }
}