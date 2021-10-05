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
        public MarkupPolicyStorageUpdater(IServiceScopeFactory scopeFactory) 
            => _scopeFactory = scopeFactory;


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<MarkupPolicyStorageUpdater>>();
                    var context = scope.ServiceProvider.GetRequiredService<EdoContext>();
                    var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

                    var lastModifiedDate = await context.MarkupPolicies
                        .OrderByDescending(p => p.Modified)
                        .Select(p => p.Modified)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (lastModifiedDate > _lastCheckingDate)
                    {
                        var markupPolicies = await context.MarkupPolicies
                            .ToListAsync(cancellationToken);

                        var markupPolicyStorage = scope.ServiceProvider.GetRequiredService<IMarkupPolicyStorage>();
                        markupPolicyStorage.Set(markupPolicies);
                        logger.LogMarkupPolicyStorageRefreshed(markupPolicies.Count);
                    }
                    
                    _lastCheckingDate = dateTimeProvider.UtcNow();
                    logger.LogMarkupPolicyStorageUpdateCompleted();
                }

                await Task.Delay(_delay, cancellationToken);
            }
        }


        public Task StopAsync(CancellationToken cancellationToken) 
            => Task.CompletedTask;


        private readonly TimeSpan _delay = TimeSpan.FromMinutes(2);
        private DateTime _lastCheckingDate = DateTime.MinValue;


        private readonly IServiceScopeFactory _scopeFactory;
    }
}