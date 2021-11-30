using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;

namespace HappyTravel.Edo.UnitTests.Tests.Infrastructure.IdempotentFunctionExecutorTests
{
    public class StubDistributedLocker : IDistributedLocker
    {
        public async Task<Result> TryAcquireLock(string key, TimeSpan duration)
        {
            await _semaphore.WaitAsync();
            if (_locks.Contains(key))
            {
                _semaphore.Release();
                return Result.Failure("Already locked");
            }
                
            _locks.Add(key);
            ScheduleLockRemove(key, duration);
            
            _semaphore.Release();
            return Result.Success();
        }


        public async Task ReleaseLock(string key)
        {
            await _semaphore.WaitAsync();
            if (_locks.Contains(key))
                _locks.Remove(key);
            _semaphore.Release();
        }


        Task ScheduleLockRemove(string key, TimeSpan removeAfter)
            => Task.Delay(removeAfter).ContinueWith(_ =>
            {
                _semaphore.Wait();
                _locks.Remove(key);
                _semaphore.Release();
            });


        private readonly HashSet<string> _locks = new();
 
        private readonly SemaphoreSlim _semaphore = new (1, 1);
    }
}