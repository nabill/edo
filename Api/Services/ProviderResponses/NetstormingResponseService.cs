using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Accommodations;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.ProviderResponses
{
    public class NetstormingResponseService : INetstormingResponseService
    {
        public NetstormingResponseService(IAccommodationService accommodationService, 
            IDataProviderClient dataProviderClient,
            IBookingRequestDataLogService bookingRequestDataLogService,
            IMemoryFlow memoryFlow,
            ICustomerContext customerContext,
            IOptions<DataProviderOptions> dataProviderOptions)
        {
            _accommodationService = accommodationService;
            _dataProviderClient = dataProviderClient;
            _dataProviderOptions = dataProviderOptions.Value;
            _bookingRequestDataLogService = bookingRequestDataLogService;
            _memoryFlow = memoryFlow;
            _customerContext = customerContext;
        }


        public async Task<Result> ProcessBookingDetailsResponse(byte[] xmlRequestData)
        {
            var xmlData = Encoding.UTF8.GetChars(xmlRequestData);
            
            if (!TryGetBookingReferenceCode(xmlData, out var bookingReferenceCode))
                return Result.Fail("Cannot extract a booking reference code from the XML request data");

            var (_, isGetBookingRequestFailure, bookingRequestData, getBookingRequestError) = await _bookingRequestDataLogService.Get(bookingReferenceCode);
            if (isGetBookingRequestFailure)
                return Result.Fail(getBookingRequestError);
            
            var (_, isGetBookingDetailsFailure, bookingDetails , bookingDetailsError) = await GetBookingDetailsFromConnector(xmlRequestData, bookingRequestData.LanguageCode);
            if (isGetBookingDetailsFailure)
                return Result.Fail(bookingDetailsError);

            var (_, isAcceptFailure, reason) = AcceptBooking(bookingDetails);
            if (isAcceptFailure)
                return Result.Ok(reason);

            await _customerContext.SetCustomerInfo(bookingRequestData.CustomerId);
            return await _accommodationService.ProcessBookingResponse(bookingDetails);
        }
        

        private async Task<Result<BookingDetails>> GetBookingDetailsFromConnector(byte[] xmlData, string languageCode)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post,
                    new Uri($"{_dataProviderOptions.Netstorming}" + "bookings/response"))
            {
                Content = new ByteArrayContent(xmlData)
            };

            var (_, isFailure, bookingDetails, error) = await _dataProviderClient.Send<BookingDetails>(httpRequestMessage, languageCode);
            return isFailure 
                ? Result.Fail<BookingDetails>(error.Detail) 
                : Result.Ok(bookingDetails);
        }


        private bool TryGetBookingReferenceCode(ReadOnlySpan<Char> sourceData, out string refCodeValue)
        {
            refCodeValue = string.Empty;
            if (sourceData.IsEmpty)
                return false;
            
            var refCodeStartIndex = sourceData.IndexOf(ReferenceCodeTagName, StringComparison.InvariantCulture);
            if (refCodeStartIndex == -1)
                return false;
            
            var sliced = sourceData.Slice(refCodeStartIndex + ReferenceCodeTagName.Length + 2);
            refCodeValue = sliced.Slice(0, sliced.IndexOf('"')).ToString();
            
            return true;
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
        private readonly IBookingRequestDataLogService _bookingRequestDataLogService;
        private readonly DataProviderOptions _dataProviderOptions;
        private readonly IMemoryFlow _memoryFlow;
        private readonly ICustomerContext _customerContext;
        
        private static readonly TimeSpan CacheExpirationPeriod = TimeSpan.FromMinutes(2);
        private const string ReferenceCodeTagName = "reference code";
        private const string CacheKeyPrefix = "NetstormingResponse";
        
    }
}