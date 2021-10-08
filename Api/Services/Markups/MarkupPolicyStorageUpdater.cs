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
    public class MarkupPolicyStorageUpdater : IHostedService
    {
        public MarkupPolicyStorageUpdater(IServiceScopeFactory scopeFactory, ILogger<MarkupPolicyStorageUpdater> logger, IMarkupPolicyStorage storage)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _storage = storage;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateStorage(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogMarkupPolicyStorageUpdateFailed(ex);
                }

                await Task.Delay(_delay, cancellationToken);
            }
        }


        public Task StopAsync(CancellationToken cancellationToken) 
            => Task.CompletedTask;


        private async Task UpdateStorage(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<EdoContext>();
            var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

            var lastModifiedDate = await context.MarkupPolicies
                .OrderByDescending(p => p.Modified)
                .Select(p => p.Modified)
                .FirstOrDefaultAsync(cancellationToken);

            if (_lastCheckingDate is null || lastModifiedDate > _lastCheckingDate)
            {
                var markupPolicies = await context.MarkupPolicies
                    .ToListAsync(cancellationToken);
                
                _storage.Set(markupPolicies);
                _logger.LogMarkupPolicyStorageRefreshed(markupPolicies.Count);
            }
                    
            _lastCheckingDate = dateTimeProvider.UtcNow();
            _logger.LogMarkupPolicyStorageUpdateCompleted();
        }


        private readonly TimeSpan _delay = TimeSpan.FromMinutes(2);
        private DateTime? _lastCheckingDate;


        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MarkupPolicyStorageUpdater> _logger;
        private readonly IMarkupPolicyStorage _storage;
    }
}