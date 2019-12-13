using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Services.Markups.Availability;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public class AvailabilityResultsCache : IAvailabilityResultsCache
    {
        public AvailabilityResultsCache(IMemoryFlow flow)
        {
            _flow = flow;
        }


        public Task Set(SingleAccommodationAvailabilityDetailsWithMarkup availabilityResponse)
        {
            _flow.Set(
                _flow.BuildKey(KeyPrefix, availabilityResponse.ResultResponse.AvailabilityId.ToString()),
                availabilityResponse,
                ExpirationPeriod);

            return Task.CompletedTask;
        }


        public Task<Result<SingleAccommodationAvailabilityDetailsWithMarkup>> Get(int id)
        {
            var isValueExist = _flow.TryGetValue<SingleAccommodationAvailabilityDetailsWithMarkup>(_flow.BuildKey(KeyPrefix, id.ToString()),
                out var availabilityResponse);

            return isValueExist
                ? Task.FromResult(Result.Fail<SingleAccommodationAvailabilityDetailsWithMarkup>($"Could not find availability with id '{id}'"))
                : Task.FromResult(Result.Ok(availabilityResponse));
        }
        

        private const string KeyPrefix = nameof(AvailabilityDetails) + "AvailabilityResults";
        private static readonly TimeSpan ExpirationPeriod = TimeSpan.FromHours(1);
        private readonly IMemoryFlow _flow;
    }
}