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
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.ProviderResponses
{
    public class NetstormingResponseService : INetstormingResponseService
    {
        public NetstormingResponseService(IAccommodationService accommodationService, ICustomerContext customerContext, IMemoryFlow memoryFlow, IOptions<DataProviderOptions> dataProviderOptions)
        {
            _accommodationService = accommodationService;
            _dataProviderOptions = dataProviderOptions.Value;
            _memoryFlow = memoryFlow;
        }


        public async Task<Result> HandleBooking(string xmlRequestData)
        {
            var (_, isResponseFailure, (bookingDetails, languageCode) , error) = await GetBookingDetailsFromConnector(xmlRequestData);
            if (isResponseFailure)
                return Result.Fail(error);

            if (IsBookingAccepted(bookingDetails.ReferenceCode, bookingDetails.Status))
                return Result.Ok("Booking response has already been accepted");

            AcceptBooking(bookingDetails.ReferenceCode, bookingDetails.Status);
            
            var (_, isBookingHandleFailure,bookingHandleError ) = await  _accommodationService.HandleBookingResponse(bookingDetails);
            return isBookingHandleFailure 
                ? Result.Fail(bookingHandleError) 
                : Result.Ok();
        }
        
        
        private async Task<Result<(BookingDetails bookingDetails, string languageCode)>> GetBookingDetailsFromConnector(string xmlBody)
        {
            var client = HttpClientFactory.Create();
            
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post,
                    new Uri($"{_dataProviderOptions.Netstorming}" + "bookings/response")) 
                {Content = new StringContent(xmlBody,
                    Encoding.UTF8, 
                    "application/xml")};
            
            var htppResponseMessage =  await client.SendAsync(httpRequestMessage);
            var content = await htppResponseMessage.Content.ReadAsStringAsync();

            if (!htppResponseMessage.IsSuccessStatusCode)
                return Result.Fail<(BookingDetails bookingDetails, string languageCode)>(content);
            
            try
            {
                var bookingDetails = JsonConvert.DeserializeObject<BookingDetails>(content);
                
                if (!htppResponseMessage.Headers.TryGetValues("Accept-language", out var languageCode) ||
                    string.IsNullOrWhiteSpace(languageCode.SingleOrDefault()))
                    Result.Fail<(BookingDetails bookingDetails, string languageCode)>("Cannot get Accept-language header from Netstorming connector");
                    
                return Result.Ok<(BookingDetails bookingDetails, string languageCode)>((bookingDetails, languageCode.SingleOrDefault()));
            }
            catch
            {
                return Result.Fail<(BookingDetails bookingDetails, string languageCode)>("Cannot handle booking response from Netstorming connector");
            }
        }


        private void AcceptBooking(string bookingReferenceCode, BookingStatusCodes status)
            => _memoryFlow.Set(_memoryFlow.BuildKey(CacheKeyPrefix, bookingReferenceCode, status.ToString()), bookingReferenceCode, CacheExpirationPeriod);
        

        private bool IsBookingAccepted(string  bookingReferenceCode, BookingStatusCodes status) 
            => _memoryFlow.TryGetValue<string>(_memoryFlow.BuildKey(CacheKeyPrefix, bookingReferenceCode, status.ToString()), out _);
        
        
        private readonly IAccommodationService _accommodationService;
        private readonly DataProviderOptions _dataProviderOptions;
        private readonly IMemoryFlow _memoryFlow;
        private const string CacheKeyPrefix = "NetstormingResponse";
        private static readonly TimeSpan CacheExpirationPeriod = TimeSpan.FromMinutes(1);
    }
}