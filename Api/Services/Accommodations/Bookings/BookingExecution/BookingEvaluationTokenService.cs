using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution
{
    public class BookingEvaluationTokenService : IBookingEvaluationTokenService
    {
        public BookingEvaluationTokenService(IBookingInfoService bookingInfoService, IDoubleFlow flow)
        {
            _bookingInfoService = bookingInfoService;
            _flow = flow;
        }


        public Task<Result<AccommodationBookingInfo>> GetByEvaluationToken(string evaluationToken, string languageCode)
        {
            return GetReferenceCode(evaluationToken)
                .Bind(GetBookingInfo);

            Task<Result<AccommodationBookingInfo>> GetBookingInfo(string referenceCode) 
                => _bookingInfoService.GetAccommodationBookingInfo(referenceCode, languageCode);
        }


        public Task SaveEvaluationTokenMapping(string evaluationToken, string referenceCode) 
            => _flow.SetAsync(GetKey(evaluationToken), referenceCode, TokenMappingLifeTime);


        private async Task<Result<string>> GetReferenceCode(string evaluationToken)
        {
            var referenceCode = await _flow.GetAsync<string>(GetKey(evaluationToken), TokenMappingLifeTime);
            return referenceCode ?? Result.Failure<string>("Could not find evaluation token mapping");
        }


        private string GetKey(string evaluationToken)
        {
            return _flow.BuildKey(nameof(BookingEvaluationTokenService), evaluationToken);
        }
        
        
        private static readonly TimeSpan TokenMappingLifeTime = TimeSpan.FromMinutes(5);
        
        private readonly IBookingInfoService _bookingInfoService;
        private readonly IDoubleFlow _flow;
    }
}