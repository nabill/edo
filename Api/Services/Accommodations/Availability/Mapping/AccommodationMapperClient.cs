using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Availabilities.Mapping;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping
{
    public class AccommodationMapperClient : IAccommodationMapperClient
    {
        public AccommodationMapperClient(IHttpClientFactory clientFactory,
            ILogger<AccommodationMapperClient> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
            _serializer = new JsonSerializer();
        }
        
        
        public async Task<Result<LocationMapping, ProblemDetails>> GetMapping(string htId, string languageCode)
        {
            var client = _clientFactory.CreateClient(HttpClientNames.MapperApi);
            try
            {
                using var response = await client.GetAsync($"{languageCode}/api/1.0/location-mappings/{htId}");

                await using var stream = await response.Content.ReadAsStreamAsync();
                using var streamReader = new StreamReader(stream);
                using var jsonTextReader = new JsonTextReader(streamReader);

                if (!response.IsSuccessStatusCode)
                {
                    var error = _serializer.Deserialize<ProblemDetails>(jsonTextReader) ??
                        ProblemDetailsBuilder.Build(response.ReasonPhrase, response.StatusCode);

                    return Result.Failure<LocationMapping, ProblemDetails>(error);
                }

                return _serializer.Deserialize<LocationMapping>(jsonTextReader);
            }
            catch (Exception ex)
            {
                _logger.LogMapperClientException(ex);
                return ProblemDetailsBuilder.Fail<LocationMapping>(ex.Message);
            }
        }
        
        
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<AccommodationMapperClient> _logger;
        private readonly JsonSerializer _serializer;
    }
}