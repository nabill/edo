using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure.DataProviders;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public class AccommodationService : IAccommodationService
    {
        public AccommodationService(IMemoryFlow flow,
            IOptions<DataProviderOptions> options,
            IDataProviderClient dataProviderClient)
        {
            _flow = flow;
            _dataProviderClient = dataProviderClient;
            _options = options.Value;
        }


        public ValueTask<Result<AccommodationDetails, ProblemDetails>> Get(string accommodationId, string languageCode)
            => _flow.GetOrSetAsync(_flow.BuildKey(nameof(AccommodationService), "Accommodations", languageCode, accommodationId),
                async () => await _dataProviderClient.Get<AccommodationDetails>(
                    new Uri($"{_options.Netstorming}accommodations/{accommodationId}", UriKind.Absolute), languageCode),
                TimeSpan.FromDays(1));


        private readonly IDataProviderClient _dataProviderClient;
        private readonly IMemoryFlow _flow;
        private readonly DataProviderOptions _options;
    }
}