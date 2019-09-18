using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public class EntityLocker : IEntityLocker
    {
        public EntityLocker(EdoContext context)
        {
            _context = context;
        }

        public async Task<Result> Acquire<TEntity>(int id, string locker)
        {
            var cts = new CancellationTokenSource(LockTimeout);
            var lockToken = Guid.NewGuid().ToString();
            var entityDescriptor = GetEntityDescriptor<TEntity>(id);
            try
            {
                while (true)
                {
                    cts.Token.ThrowIfCancellationRequested();
                    var lockTaken = await _context.TryAddEntityLock(entityDescriptor, locker, lockToken);
                    if (lockTaken)
                        return Result.Ok();

                    await Task.Delay(GetRandomDelay(), cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                return Result.Fail("Lock timeout");
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
        private const int LockTimeoutMilliseconds = 2000;
        
        private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(LockTimeoutMilliseconds);

        private readonly EdoContext _context;
        private readonly Random _random = new Random();
    }
}