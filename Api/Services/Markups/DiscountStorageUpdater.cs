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
    public class DiscountStorageUpdater : BackgroundService
    {
        public DiscountStorageUpdater(IServiceScopeFactory scopeFactory, IDiscountStorage storage)
        {
            _scopeFactory = scopeFactory;
            _storage = storage;
        }
        
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<DiscountStorageUpdater>>();
                
                try
                {
                    await UpdateStorage(scope, logger, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogDiscountStorageUpdateFailed(ex);
                }

                await Task.Delay(_delay, stoppingToken);
            }
        }
        
        
        private async Task UpdateStorage(IServiceScope scope, ILogger<DiscountStorageUpdater> logger, CancellationToken cancellationToken)
        {
            await using var context = scope.ServiceProvider.GetRequiredService<EdoContext>();
            var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

            var lastModifiedDate = await context.Discounts
                .OrderByDescending(p => p.Modified)
                .Select(p => p.Modified)
                .FirstOrDefaultAsync(cancellationToken);

            var lastRecordsCount = await context.Discounts.CountAsync(cancellationToken);

            if (_lastCheckingDate is null || lastModifiedDate > _lastCheckingDate || lastRecordsCount != _lastRecordsCount )
            {
                var discounts = await context.Discounts
                    .ToListAsync(cancellationToken);
                
                _storage.Set(discounts);
                logger.LogDiscountStorageRefreshed(discounts.Count);
            }
                    
            _lastCheckingDate = dateTimeProvider.UtcNow();
            _lastRecordsCount = lastRecordsCount;
            logger.LogDiscountStorageUpdateCompleted();
        }
        
        
        private readonly TimeSpan _delay = TimeSpan.FromMinutes(2);
        private DateTime? _lastCheckingDate;
        private int? _lastRecordsCount;


        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IDiscountStorage _storage;
    }
}