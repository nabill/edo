using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.Infrastructure.Locking
{
    public class IdempotentFunctionExecutor
    {
        public IdempotentFunctionExecutor(IDistributedLocker locker)
        {
            _locker = locker;
        }


        public async Task<TResult> Execute<TResult>(Func<Task<TResult>> executingFunction, Func<Task<TResult>> getResultFunction, 
            string operationKey, TimeSpan maximumDuration)
        {
            var key = $"{nameof(IdempotentFunctionExecutor)}::{operationKey}";
            // If lock is acquired, the function is executing as normal
            var (isInitialLockSuccess, _, _) = await _locker.TryAcquireLock(key, maximumDuration);
            if (isInitialLockSuccess)
            {
                var result = await executingFunction();
                await _locker.ReleaseLock(key);
                return result;
            }

            // Waiting until lock is released to get the result
            var attemptCount = Convert.ToInt32((maximumDuration + BufferStepDuration)/ StepDuration);
            for (var i = 0; i < attemptCount; i++)
            {
                await Task.Delay(StepDuration);
                var (isLockReleased, _, _) = await _locker.TryAcquireLock(operationKey, TimeSpan.MinValue);
                if (isLockReleased)
                    break;
            }

            return await getResultFunction();
        }


        private static readonly TimeSpan StepDuration = TimeSpan.FromMilliseconds(500);
        
        private static readonly TimeSpan BufferStepDuration = TimeSpan.FromMilliseconds(600);
        
        private readonly IDistributedLocker _locker;
    }
}