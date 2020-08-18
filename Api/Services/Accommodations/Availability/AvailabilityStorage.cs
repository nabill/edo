using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public class AvailabilityStorage : IAvailabilityStorage
    {
        public AvailabilityStorage(IDistributedFlow distributedFlow, IMemoryFlow memoryFlow)
        {
            _distributedFlow = distributedFlow;
            _memoryFlow = memoryFlow;
        }


        public Task<(DataProviders DataProvider, TObject Result)[]> GetProviderResults<TObject>(Guid searchId, List<DataProviders> dataProviders, bool isCachingEnabled = false)
        {
            var providerTasks = dataProviders
                .Select(async p =>
                {
                    var key = BuildKey<TObject>(searchId, p);
                    return (
                        ProviderKey: p,
                        Object: await Get(key, isCachingEnabled)
                    );
                })
                .ToArray();

            return Task.WhenAll(providerTasks);


            async ValueTask<TObject> Get(string key, bool isCachingEnabled)
            {
                if(!isCachingEnabled)
                    return await _distributedFlow.GetAsync<TObject>(key);
                
                if (_memoryFlow.TryGetValue(key, out TObject value))
                    return value;
                    
                value = await _distributedFlow.GetAsync<TObject>(key);
                if(value != null && !value.Equals(default))
                    _memoryFlow.Set(key, value, CacheExpirationTime);
                
                return value;
            }
        }


        public Task SaveObject<TObjectType>(Guid searchId, TObjectType @object, DataProviders? dataProvider = null)
        {
            var key = BuildKey<TObjectType>(searchId, dataProvider);
            return _distributedFlow.SetAsync(key, @object, CacheExpirationTime);
        }


        private string BuildKey<TObjectType>(Guid searchId, DataProviders? dataProvider = null)
            => _distributedFlow.BuildKey(nameof(AvailabilityStorage),
                searchId.ToString(),
                typeof(TObjectType).Name,
                dataProvider?.ToString() ?? string.Empty);


        private static readonly TimeSpan CacheExpirationTime = TimeSpan.FromMinutes(15);

        private readonly IDistributedFlow _distributedFlow;
        private readonly IMemoryFlow _memoryFlow;
    }
}