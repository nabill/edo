using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Availabilities;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public class AvailabilityRequestStorage : IAvailabilityRequestStorage
    {
        public AvailabilityRequestStorage(IDoubleFlow flow, IOptionsMonitor<AvailabilityRequestStorageOptions> options, IDateTimeProvider dateTimeProvider)
        {
            _flow = flow;
            _options = options;
            _dateTimeProvider = dateTimeProvider;
        }


        public async Task Set(Guid searchId, AvailabilityRequest request)
        {
           await _flow.SetAsync(BuildKey(searchId), request, _options.CurrentValue.StorageLifeTime);
           await _flow.SetAsync(BuildKeyStartedTime(searchId), _dateTimeProvider.UtcNow(), _options.CurrentValue.StorageLifeTime);
        }


        public async Task<Result<AvailabilityRequest>> Get(Guid searchId)
        {
            var key = BuildKey(searchId);
            var request = await _flow.GetAsync<AvailabilityRequest?>(key, _options.CurrentValue.StorageLifeTime);
            return request ?? Result.Failure<AvailabilityRequest>("Could not find search request in cache");
        }

        public async Task<Result<DateTimeOffset>> GetStartedTime(Guid searchId)
        {
            var key = BuildKeyStartedTime(searchId);
            var request = await _flow.GetAsync<DateTimeOffset?>(key, _options.CurrentValue.StorageLifeTime);
            return request ?? Result.Failure<DateTimeOffset>("Could not find search request in cache");
        }

        private string BuildKey(Guid searchId) 
            => _flow.BuildKey(nameof(AvailabilityRequestStorage), searchId.ToString());
        
        private string BuildKeyStartedTime(Guid searchId) 
            => _flow.BuildKey(nameof(AvailabilityRequestStorage), "StartedTime", searchId.ToString());


        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IOptionsMonitor<AvailabilityRequestStorageOptions> _options;
        private readonly IDoubleFlow _flow;
    }
}