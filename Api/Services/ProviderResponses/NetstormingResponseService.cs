using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.ProviderResponses
{
    public class NetstormingResponseService : INetstormingResponseService
    {
        public NetstormingResponseService(IOptions<DataProviderOptions> dataProviderOptions)
        {
            _dataProviderOptions = dataProviderOptions.Value;
        }


        public async Task<Result<ProblemDetails>> HandleBookingResponse(string xml)
        {
            var (_, isResponseFailure, response, error) = await HandleNetstormingResponse(xml);
            
            var (bookingDetails, languageCode) = response;
            
            throw new NotImplementedException();
        }


        private async Task<Result<(BookingDetails bookingDetails, string languageCode)>> HandleNetstormingResponse(string xmlBody)
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
        

        private readonly DataProviderOptions _dataProviderOptions;
    }
}