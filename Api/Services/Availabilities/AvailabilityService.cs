using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Services.Locations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HappyTravel.Edo.Api.Services.Availabilities
{
    public class AvailabilityService : IAvailabilityService
    {
        public AvailabilityService(ILoggerFactory loggerFactory, IHttpClientFactory clientFactory, ILocationService locationService)
        {
            _clientFactory = clientFactory;
            _locationService = locationService;
            _logger = loggerFactory.CreateLogger<AvailabilityService>();

            _serializer = new JsonSerializer();
        }


        public async ValueTask<Result<AvailabilityResponse, ProblemDetails>> Get(AvailabilityRequest request, string languageCode)
        {
            var (_, isFailure, location, error) = await _locationService.Get(request.Location, languageCode);
            if (isFailure)
                return Result.Fail<AvailabilityResponse, ProblemDetails>(error);

            return await CheckAvailability(new InnerAvailabilityRequest(request, location));
        }


        private async Task<Result<AvailabilityResponse, ProblemDetails>> CheckAvailability(InnerAvailabilityRequest request)
        {
            try
            {
                var requestContent = JsonConvert.SerializeObject(request);

                using (var client = _clientFactory.CreateClient(HttpClientNames.NetstormingConnector))
                using (var response = await client.PostAsync("hotels/availability", new StringContent(requestContent, Encoding.UTF8, "application/json")))
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var streamReader = new StreamReader(stream))
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        var error = _serializer.Deserialize<ProblemDetails>(jsonTextReader);
                        return Result.Fail<AvailabilityResponse, ProblemDetails>(error);
                    }

                    var availabilityResponse = _serializer.Deserialize<AvailabilityResponse>(jsonTextReader);
                    return Result.Ok<AvailabilityResponse, ProblemDetails>(availabilityResponse);
                }
            }
            catch (Exception ex)
            {
                _logger.LogAvailabilityCheckException(ex);
                throw;
            }
        }


        private readonly IHttpClientFactory _clientFactory;
        private readonly ILocationService _locationService;
        private readonly ILogger<AvailabilityService> _logger;
        private readonly JsonSerializer _serializer;
    }
}