using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Markup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class MarkupPolicyStorage : IMarkupPolicyStorage
    {
        public MarkupPolicyStorage(IServiceScopeFactory scopeFactory) 
            => _scopeFactory = scopeFactory;


        public async Task<List<MarkupPolicy>> Get(Func<MarkupPolicy, bool> predicate)
        {
            if (_lastUpdate.Add(_refreshDelay) <= DateTime.Now)
                await Refresh();

            _lock.EnterReadLock();
            
            try
            {
                return _storage.Where(predicate).ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        
        
        private async Task Refresh()
        {
            _lock.EnterWriteLock();

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<EdoContext>();
                var policies = await context.MarkupPolicies.ToListAsync();
                
                _storage.Clear();
                _storage.AddRange(policies);
                _lastUpdate = DateTime.UtcNow;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }


        private DateTime _lastUpdate = DateTime.UtcNow;
        private readonly TimeSpan _refreshDelay = TimeSpan.FromMinutes(2);
        private readonly List<MarkupPolicy> _storage = new();
        private readonly ReaderWriterLockSlim _lock = new();


        private readonly IServiceScopeFactory _scopeFactory;
    }
}