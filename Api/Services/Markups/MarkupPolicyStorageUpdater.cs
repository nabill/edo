using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class MarkupPolicyStorageUpdater : BackgroundService
    {
        public MarkupPolicyStorageUpdater(IServiceScopeFactory scopeFactory, IMarkupPolicyStorage storage)
        {
            _scopeFactory = scopeFactory;
            _storage = storage;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<MarkupPolicyStorageUpdater>>();
                
                try
                {
                    await UpdateStorage(scope, logger, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogMarkupPolicyStorageUpdateFailed(ex);
                }

                await Task.Delay(_delay, stoppingToken);
            }
        }


        private async Task UpdateStorage(IServiceScope scope, ILogger<MarkupPolicyStorageUpdater> logger, CancellationToken cancellationToken)
        {
            await using var context = scope.ServiceProvider.GetRequiredService<EdoContext>();
            var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

            var lastModifiedDate = await context.MarkupPolicies
                .OrderByDescending(p => p.Modified)
                .Select(p => p.Modified)
                .FirstOrDefaultAsync(cancellationToken);

            var lastRecordsCount = await context.MarkupPolicies.CountAsync(cancellationToken);

            if (_lastCheckingDate is null || lastModifiedDate > _lastCheckingDate || lastRecordsCount != _lastRecordsCount )
            {
                var markupPolicies = await context.MarkupPolicies
                    .ToListAsync(cancellationToken);
                
                _storage.Set(markupPolicies);
                logger.LogMarkupPolicyStorageRefreshed(markupPolicies.Count);
            }
                    
            _lastCheckingDate = dateTimeProvider.UtcNow();
            _lastRecordsCount = lastRecordsCount;
            logger.LogMarkupPolicyStorageUpdateCompleted();
        }


        private readonly TimeSpan _delay = TimeSpan.FromMinutes(2);
        private DateTime? _lastCheckingDate;
        private int? _lastRecordsCount;


        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMarkupPolicyStorage _storage;
    }
}