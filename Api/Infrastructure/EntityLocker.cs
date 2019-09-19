using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data;
using Polly;
using Polly.Retry;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public class EntityLocker : IEntityLocker
    {
        public EntityLocker(EdoContext context)
        {
            _context = context;
        }

        public async Task<Result> Acquire<TEntity>(int entityId, string locker)
        {
            var entityDescriptor = GetEntityDescriptor<TEntity>(entityId);
            var token = Guid.NewGuid().ToString();

            var lockTaken = await GetRetryPolicy()
                .ExecuteAsync(() => _context.TryAddEntityLock(entityDescriptor, locker, token));

            return lockTaken
                ? Result.Ok()
                : Result.Fail($"Could not acquire lock for entity with id: {entityId}");
            
            RetryPolicy<bool> GetRetryPolicy()
            {
                return Policy
                    .HandleResult(false)
                    .WaitAndRetryAsync(MaxLockRetryCount, attemptNumber => GetRandomDelay());
            }
            
            TimeSpan GetRandomDelay()
            {
                return TimeSpan.FromMilliseconds(_random.Next(MinRetryPeriodMilliseconds, 
                    MaxRetryPeriodMilliseconds));
            }
        }

        public Task Release<TEntity>(int entityId)
        {
            return _context.RemoveEntityLock(GetEntityDescriptor<TEntity>(entityId));
        }
        
        private static string GetEntityDescriptor<TEntity>(int id) => $"{typeof(TEntity).Name}_{id}";

        private const int MinRetryPeriodMilliseconds = 20;
        private const int MaxRetryPeriodMilliseconds = 100;
        private const int MaxLockRetryCount = 20;

        private readonly EdoContext _context;
        private readonly Random _random = new Random();
    }
}