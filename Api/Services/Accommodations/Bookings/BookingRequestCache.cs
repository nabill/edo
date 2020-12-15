using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Models.Bookings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public class BookingRequestCache : IBookingRequestCache
    {
        public BookingRequestCache(IDoubleFlow flow)
        {
            _flow = flow;
        }
        
        
        public Task Set(string referenceCode, (AccommodationBookingRequest request, string availabilityId) requestInfo)
        {
            var key = BuildKey(referenceCode);
            return _flow.SetAsync(key, requestInfo, RequestCacheLifeTime);
        }


        public async Task<Result<(AccommodationBookingRequest request, string availabilityId)>> Get(string referenceCode)
        {
            var key = BuildKey(referenceCode);
            var request = await _flow.GetAsync<(AccommodationBookingRequest request, string availabilityId)?>(key, RequestCacheLifeTime);
            return request ?? Result.Failure<(AccommodationBookingRequest, string)>($"Could not get request for booking '{referenceCode}'");
        }


        private string BuildKey(string referenceCode) => _flow.BuildKey(nameof(BookingRequestCache), referenceCode);

        
        private static readonly TimeSpan RequestCacheLifeTime = TimeSpan.FromMinutes(15);
        
        private readonly IDoubleFlow _flow;
    }
}