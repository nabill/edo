using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Hotels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Availabilities
{
    public class HotelService : IHotelService
    {
        public HotelService(IMemoryFlow flow, IOptions<DataProviderOptions> options, IDataProviderClient netClient)
        {
            _flow = flow;
            _netClient = netClient;
            _options = options.Value;
        }


        public ValueTask<Result<RichHotelDetails, ProblemDetails>> Get(string hotelId, string languageCode)
            => _flow.GetOrSetAsync(_flow.BuildKey(nameof(HotelService), "Hotels", languageCode, hotelId),
                async () => await _netClient.Get<RichHotelDetails>(new Uri($"{_options.Netstorming}hotels/{hotelId}", UriKind.Absolute), languageCode),
                TimeSpan.FromDays(1));


        private readonly IMemoryFlow _flow;
        private readonly IDataProviderClient _netClient;
        private readonly DataProviderOptions _options;
    }
}
