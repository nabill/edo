using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.Locking;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Infrastructure.IdempotentFunctionExecutorTests
{
    public class IdempotenceTests
    {
        [Fact]
        public async Task Requests_in_parallel_execute_function_once()
        {
            var operationKey = "operation_key";
            var tasksBag = new ConcurrentBag<Task>();
            var executingFunctionCallCount = 0;
            var getResultFunctionCallCount = 0;
            var locker = new StubDistributedLocker();

            Parallel.For(0, 100, _ =>
            {
                var executor = new IdempotentFunctionExecutor(locker);
                var task = executor.Execute(() => Increment(ref executingFunctionCallCount),
                    () => Increment(ref getResultFunctionCallCount),
                    operationKey,
                    TimeSpan.FromSeconds(1));
                
                tasksBag.Add(task);
            });
            await Task.WhenAll(tasksBag);
                
            Assert.Equal(1, executingFunctionCallCount);
            Assert.Equal(99, getResultFunctionCallCount);
        }
        
        
        [Fact]
        public async Task Function_executed_once_blocks_gets_results_for_further_executions()
        {
            var operationKey = "operation_key";
            var executor = new IdempotentFunctionExecutor(new StubDistributedLocker());
            var executingFunctionCallCount = 0;
            var getResultFunctionCallCount = 0;
            
            await Execute();
            await Execute();
            await Execute();
                
            Assert.Equal(1, executingFunctionCallCount);
            Assert.Equal(2, getResultFunctionCallCount);

            
            Task Execute()
            {
                return executor.Execute(() => Increment(ref executingFunctionCallCount),
                    () => Increment(ref getResultFunctionCallCount),
                    operationKey,
                    TimeSpan.FromSeconds(5));
            }
        }


        private static Task<int> Increment(ref int counter)
        {
            Interlocked.Increment(ref counter);
            return Task.FromResult(0);
        }
    }
}