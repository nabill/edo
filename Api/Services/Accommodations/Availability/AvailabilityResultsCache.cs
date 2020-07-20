using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public class AvailabilityResultsCache : IAvailabilityResultsCache
    {
        public AvailabilityResultsCache(IDistributedFlow flow)
        {
            _flow = flow;
        }


        public Task Set(DataProviders dataProvider, DataWithMarkup<SingleAccommodationAvailabilityDetailsWithDeadline> availabilityResponse)
        {
            _flow.Set(
                _flow.BuildKey(KeyPrefix, dataProvider.ToString(), availabilityResponse.Data.AvailabilityId),
                availabilityResponse,
                ExpirationPeriod);

            return Task.CompletedTask;
        }


        public Task<Result<DataWithMarkup<SingleAccommodationAvailabilityDetailsWithDeadline>>> Get(DataProviders dataProvider, string id)
        {
            var isValueExist = _flow.TryGetValue<DataWithMarkup<SingleAccommodationAvailabilityDetailsWithDeadline>>(_flow.BuildKey(KeyPrefix, dataProvider.ToString(), id),
                out var availabilityResponse);

            return isValueExist
                ? Task.FromResult(Result.Ok(availabilityResponse))
                : Task.FromResult(Result.Failure<DataWithMarkup<SingleAccommodationAvailabilityDetailsWithDeadline>>($"Could not find availability with id '{id}'"));
        }


        private const string KeyPrefix = nameof(AvailabilityDetails) + "AvailabilityResults";
        private static readonly TimeSpan ExpirationPeriod = TimeSpan.FromHours(1);
        private readonly IDistributedFlow _flow;
    }
}