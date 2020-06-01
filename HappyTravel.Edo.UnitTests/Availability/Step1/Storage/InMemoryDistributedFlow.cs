using System;
using System.Threading;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace HappyTravel.Edo.UnitTests.Availability.Step1.Storage
{
    internal class InMemoryDistributedFlow : IDistributedFlow
    {
        public Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = new CancellationToken())
        {
            _memoryFlow.TryGetValue<T>(key, out var value);
            return Task.FromResult(value);
        }
        
        
        public Task SetAsync<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow, CancellationToken cancellationToken = new CancellationToken())
        {
            _memoryFlow.Set(key, value, absoluteExpirationRelativeToNow);
            return Task.CompletedTask;
        }
        
        public FlowOptions Options { get; } = new FlowOptions();
        
        private readonly MemoryFlow _memoryFlow = new MemoryFlow(new MemoryCache(Microsoft.Extensions.Options.Options.Create(new MemoryCacheOptions())));


        #region Not implemented

        public T GetOrSet<T>(string key, Func<T> getValueFunction, TimeSpan absoluteExpirationRelativeToNow)
        {
            throw new NotImplementedException();
        }


        public T GetOrSet<T>(string key, Func<T> getValueFunction, DistributedCacheEntryOptions options)
        {
            throw new NotImplementedException();
        }


        public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction, TimeSpan absoluteExpirationRelativeToNow,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }


        public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction, DistributedCacheEntryOptions options, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }


        public void Refresh(string key)
        {
            throw new NotImplementedException();
        }


        public Task RefreshAsync(string key, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }


        public void Remove(string key)
        {
            throw new NotImplementedException();
        }


        public Task RemoveAsync(string key, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }


        public void Set<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow)
        {
            throw new NotImplementedException();
        }


        public void Set<T>(string key, T value, DistributedCacheEntryOptions options)
        {
            throw new NotImplementedException();
        }


        public Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }


        public bool TryGetValue<T>(string key, out T value)
        {
            throw new NotImplementedException();
        }
        
        
        public IDistributedCache Instance => throw new NotImplementedException();

        #endregion
    }
}