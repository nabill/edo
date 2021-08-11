using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public class AccommodationService : IAccommodationService
    {
        public AccommodationService(IDoubleFlow flow,
            IAccommodationMapperClient mapperClient)
        {
            _flow = flow;
            _mapperClient = mapperClient;
        }


        public async Task<Result<Accommodation, ProblemDetails>> Get(string htId, string languageCode)
        {
            if (string.IsNullOrEmpty(htId))
                return ProblemDetailsBuilder.Fail<Accommodation>("Could not get accommodation data");

            var key = _flow.BuildKey(nameof(AccommodationService), nameof(Get), languageCode, htId);
            var cachedAccommodation = await _flow.GetAsync<Accommodation>(key, AccommodationCacheLifeTime);
            if (string.IsNullOrWhiteSpace(cachedAccommodation.Id))
            {
                var (_, isFailure, mapperAccommodation, error) = await _mapperClient.GetAccommodation(htId, languageCode);
                if (isFailure)
                    return ProblemDetailsBuilder.Fail<Accommodation>(error.Detail);

                var accommodation = mapperAccommodation.ToEdoContract();
                await _flow.SetAsync(key, accommodation, AccommodationCacheLifeTime);
                return accommodation;
            }

            return cachedAccommodation;
        }


        private static readonly TimeSpan AccommodationCacheLifeTime = TimeSpan.FromHours(4);
        
        private readonly IDoubleFlow _flow;
        private readonly IAccommodationMapperClient _mapperClient;
    }
}