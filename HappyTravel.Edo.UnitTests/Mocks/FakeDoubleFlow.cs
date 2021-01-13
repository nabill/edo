using System;
using System.Threading;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace HappyTravel.Edo.UnitTests.Mocks
{
    public class FakeDoubleFlow : IDoubleFlow
    {
        public ValueTask<T> GetAsync<T>(string key, TimeSpan absoluteDistributedExpirationRelativeToNow, CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();


        public T GetOrSet<T>(string key, Func<T> getValueFunction, DistributedCacheEntryOptions distributedOptions, MemoryCacheEntryOptions? memoryOptions = null) => throw new NotImplementedException();


        public T GetOrSet<T>(string key, Func<T> getValueFunction, TimeSpan absoluteDistributedExpirationRelativeToNow) => getValueFunction();


        public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction, DistributedCacheEntryOptions distributedOptions, MemoryCacheEntryOptions? memoryOptions = null,
            CancellationToken cancellationToken = new CancellationToken())
            => getValueFunction();


        public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction, TimeSpan absoluteDistributedExpirationRelativeToNow,
            CancellationToken cancellationToken = new CancellationToken())
            => getValueFunction();


        public void Refresh(string key)
        {
            throw new NotImplementedException();
        }


        public Task RefreshAsync(string key, CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();


        public void Remove(string key)
        {
            throw new NotImplementedException();
        }


        public Task RemoveAsync(string key, CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();


        public void Set<T>(string key, T value, DistributedCacheEntryOptions distributedOptions, MemoryCacheEntryOptions? memoryOptions = null)
        {
            throw new NotImplementedException();
        }


        public void Set<T>(string key, T value, TimeSpan absoluteDistributedExpirationRelativeToNow)
        {
            throw new NotImplementedException();
        }


        public Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions distributedOptions, MemoryCacheEntryOptions? memoryOptions = null,
            CancellationToken cancellationToken = new CancellationToken())
            => throw new NotImplementedException();


        public Task SetAsync<T>(string key, T value, TimeSpan absoluteDistributedExpirationRelativeToNow, CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();


        public bool TryGetValue<T>(string key, out T value, MemoryCacheEntryOptions memoryOptions) => throw new NotImplementedException();


        public bool TryGetValue<T>(string key, out T value, TimeSpan absoluteDistributedExpirationRelativeToNow) => throw new NotImplementedException();


        public IDistributedCache? DistributedInstance { get; }
        public IMemoryCache MemoryInstance => new MemoryCache(new MemoryCacheOptions());
        public FlowOptions Options => new FlowOptions();
    }
}