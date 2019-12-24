using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Markups.Availability;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public class BookingRequestCache: IBookingRequestCache
    {
        public BookingRequestCache(IMemoryFlow flow)
        {
            _flow = flow;
        }


        public Result Set(AccommodationBookingRequest bookingRequest, string bookingReferenceCode)
        {
            _flow.Set( _flow.BuildKey(KeyPrefix, bookingReferenceCode), bookingRequest, ExpirationPeriod);
            return Result.Ok();
        }
    

        public Result<AccommodationBookingRequest> Get(string bookingReferenceCode)
        {
            var isValueExist = _flow.TryGetValue<AccommodationBookingRequest>(_flow.BuildKey(KeyPrefix, bookingReferenceCode),
                out var bookingRequest);

            return isValueExist
                ? Result.Ok(bookingRequest)
                : Result.Fail<AccommodationBookingRequest>($"Could not find the booking request with the reference code '{bookingReferenceCode}'");
        }
        

        private const string KeyPrefix = nameof(AccommodationBookingRequest) + "BookingRequest";
        private static readonly TimeSpan ExpirationPeriod = TimeSpan.FromHours(1);
        private readonly IMemoryFlow _flow;
    }
}