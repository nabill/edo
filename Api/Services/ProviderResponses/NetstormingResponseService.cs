using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure.DataProviders;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Services.Accommodations;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.ProviderResponses
{
    public class NetstormingResponseService : INetstormingResponseService
    {
        public NetstormingResponseService(IAccommodationService accommodationService, 
            IDataProviderClient dataProviderClient,
            IMemoryFlow memoryFlow,
            ICustomerContext customerContext,
            IAccommodationBookingManager accommodationBookingManager,
            IBookingService bookingService,
            IOptions<DataProviderOptions> dataProviderOptions)
        {
            _accommodationService = accommodationService;
            _dataProviderClient = dataProviderClient;
            _dataProviderOptions = dataProviderOptions.Value;
            _memoryFlow = memoryFlow;
            _customerContext = customerContext;
            _accommodationBookingManager = accommodationBookingManager;
            _bookingService = bookingService;
        }


        public async Task<Result> ProcessBookingDetailsResponse(byte[] xmlRequestData)
        {
            var (_, isGetBookingDetailsFailure, bookingDetails , bookingDetailsError) = await GetBookingDetailsFromConnector(xmlRequestData);
            if (isGetBookingDetailsFailure)
                return Result.Fail(bookingDetailsError);

            var (_, isAcceptFailure, reason) = AcceptBooking(bookingDetails);
            if (isAcceptFailure)
                return Result.Ok(reason);

            var (_, isGetBookingFailure, booking, getBookingError) =
                await _accommodationBookingManager.Get(bookingDetails.ReferenceCode);
            
            if (isGetBookingFailure)
                return Result.Fail(getBookingError);
            
            await _customerContext.SetCustomerInfo(booking.CustomerId);
            
            return await _bookingService.ProcessResponse(bookingDetails, booking);
        }
        

        private async Task<Result<BookingDetails>> GetBookingDetailsFromConnector(byte[] xmlData)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post,
                    new Uri($"{_dataProviderOptions.Netstorming}" + "bookings/response"))
            {
                Content = new ByteArrayContent(xmlData)
            };

            var (_, isFailure, bookingDetails, error) = await _dataProviderClient.Send<BookingDetails>(httpRequestMessage);
            return isFailure 
                ? Result.Fail<BookingDetails>(error.Detail) 
                : Result.Ok(bookingDetails);
        }
        
        
        private Result AcceptBooking(BookingDetails bookingDetails)
        {
            if (IsBookingAccepted(bookingDetails.ReferenceCode, bookingDetails.Status))
                return Result.Fail("Response has already been accepted");
            _memoryFlow.Set(_memoryFlow.BuildKey(CacheKeyPrefix, bookingDetails.ReferenceCode, bookingDetails.Status.ToString()), bookingDetails.ReferenceCode, CacheExpirationPeriod);
            
            return Result.Ok();
        }


        private bool IsBookingAccepted(string  bookingReferenceCode, BookingStatusCodes status) 
            => _memoryFlow.TryGetValue<string>(_memoryFlow.BuildKey(CacheKeyPrefix, bookingReferenceCode, status.ToString()), out _);


        private readonly IAccommodationService _accommodationService;
        private readonly IDataProviderClient _dataProviderClient;
        private readonly IAccommodationBookingManager _accommodationBookingManager;
        private readonly IBookingService _bookingService;
        private readonly DataProviderOptions _dataProviderOptions;
        private readonly IMemoryFlow _memoryFlow;
        private readonly ICustomerContext _customerContext;
        
        private static readonly TimeSpan CacheExpirationPeriod = TimeSpan.FromMinutes(2);
        private const string CacheKeyPrefix = "NetstormingResponse";
        
    }
}