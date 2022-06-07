using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure.Locking;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution
{
    public class IdempotentBookingExecutor
    {
        public IdempotentBookingExecutor(IdempotentFunctionExecutor functionExecutor, IDoubleFlow flow, IBookingInfoService bookingInfoService, 
            IEvaluationTokenStorage tokenStorage)
        {
            _functionExecutor = functionExecutor;
            _flow = flow;
            _bookingInfoService = bookingInfoService;
            _tokenStorage = tokenStorage;
        }


        public async Task<Result<AccommodationBookingInfo>> Execute(AccommodationBookingRequest request, Func<Task<Result<AccommodationBookingInfo>>> bookingFunction, string languageCode)
        {
            return await _functionExecutor.Execute(executingFunction: async () =>
                {
                    var tokenIsExists = await _tokenStorage.IsExists(request.EvaluationToken, request.RoomContractSetId);
                    if (!tokenIsExists)
                        return Result.Failure<AccommodationBookingInfo>("Evaluation token is not exists");
                    
                    return await bookingFunction().Tap(SaveEvaluationTokenMapping);
                },
                getResultFunction: () => GetBookingByEvaluationToken(request.EvaluationToken, languageCode),
                operationKey: request.EvaluationToken,
                maximumDuration: MaximumBookingDuration);


            Task SaveEvaluationTokenMapping(AccommodationBookingInfo bookingInfo)
            {
                var key = GetEvaluationTokenKey(request.EvaluationToken);
                return _flow.SetAsync(key, bookingInfo.BookingDetails.ReferenceCode, EvaluationTokenMappingLifeTime);
            }


            async Task<Result<AccommodationBookingInfo>> GetBookingByEvaluationToken(string evaluationToken, string languageCode)
            {
                var key = GetEvaluationTokenKey(request.EvaluationToken);
                var referenceCode = await _flow.GetAsync<string>(key, EvaluationTokenMappingLifeTime);
                if (referenceCode is null)
                    return Result.Failure<AccommodationBookingInfo>("Could not find booking reference code");

                return await _bookingInfoService.GetAccommodationBookingInfo(referenceCode, languageCode);
            }


            string GetEvaluationTokenKey(string evaluationToken) 
                => _flow.BuildKey(nameof(IdempotentBookingExecutor), evaluationToken);
        }
        

        private static readonly TimeSpan MaximumBookingDuration = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan EvaluationTokenMappingLifeTime = MaximumBookingDuration + TimeSpan.FromMinutes(1);
        private readonly IdempotentFunctionExecutor _functionExecutor;
        private readonly IDoubleFlow _flow;
        private readonly IBookingInfoService _bookingInfoService;
        private readonly IEvaluationTokenStorage _tokenStorage;
    }
}