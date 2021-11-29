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
            // Lock key, indicating operation started
            var operationStartedKey = GetLockKey(LockKind.OperationStarted);
            // Lock key, indicating operation in progress
            var operationInProgressKey = GetLockKey(LockKind.OperationExecuting);
            
            // If operation is not already started and not executing right now, executing operation as normal
            if (await AcquireLock(operationStartedKey, maximumDuration) && await AcquireLock(operationInProgressKey, maximumDuration))
            {
                var result = await executingFunction();
                await _locker.ReleaseLock(operationInProgressKey);
                return result;
            }

            // If operation started already, we'll wait its finish to get the result
            var attemptCount = Convert.ToInt32((maximumDuration + BufferStepDuration) / StepDuration);
            for (var i = 0; i < attemptCount; i++)
            {
                var isOperationEnded = await AcquireLock(operationInProgressKey, TimeSpan.MinValue);
                if (isOperationEnded)
                    break;
                
                await Task.Delay(StepDuration);
            }

            return await getResultFunction();


            async Task<bool> AcquireLock(string key, TimeSpan? duration)
            {
               var (isLockTaken, _, _) = await _locker.TryAcquireLock(key, duration ?? TimeSpan.MinValue);
               return isLockTaken;
            }


            string GetLockKey(LockKind kind) 
                => $"{nameof(IdempotentFunctionExecutor)}::{operationKey}::{kind}";
        }


        private static readonly TimeSpan StepDuration = TimeSpan.FromMilliseconds(500);
        
        private static readonly TimeSpan BufferStepDuration = TimeSpan.FromMilliseconds(600);
        
        private readonly IDistributedLocker _locker;
        
        private enum LockKind
        {
            OperationStarted,
            OperationExecuting
        }
    }
}