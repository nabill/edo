using System;
using System.Linq;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Common.Enums;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public class AvailabilityStorage : IAvailabilityStorage
    {
        public AvailabilityStorage(IDistributedFlow distributedFlow,
            IOptions<DataProviderOptions> options)
        {
            _distributedFlow = distributedFlow;
            _providerOptions = options.Value;
        }


        public Task<(DataProviders DataProvider, TObject Result)[]> GetProviderResults<TObject>(Guid searchId)
        {
            var providerTasks = _providerOptions
                .EnabledProviders
                .Select(async p =>
                {
                    var key = BuildKey<TObject>(searchId, p);
                    return (
                        ProviderKey: p,
                        Object: await _distributedFlow.GetAsync<TObject>(key)
                    );
                })
                .ToArray();

            return Task.WhenAll(providerTasks);
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
        private readonly DataProviderOptions _providerOptions;
    }
}