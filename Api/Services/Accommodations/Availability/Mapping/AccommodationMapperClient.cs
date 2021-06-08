using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.MapperContracts.Internal.Mappings;
using HappyTravel.MapperContracts.Public.Accommodations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping
{
    public class AccommodationMapperClient : IAccommodationMapperClient
    {
        public AccommodationMapperClient(IHttpClientFactory clientFactory,
            ILogger<AccommodationMapperClient> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }
        
        
        public async Task<Result<List<LocationMapping>, ProblemDetails>> GetMappings(List<string> htIds, string languageCode)
        {
            if (!htIds.Any())
                return ProblemDetailsBuilder.Fail<List<LocationMapping>>("Could not get mapping for an empty ids list");
                    
            var client = _clientFactory.CreateClient(HttpClientNames.MapperApi);
            try
            {
                var htIdQuery = string.Join("&", htIds.Select(h => $"htIds={h}"));
                using var response = await client.GetAsync($"api/1.0/location-mappings?{htIdQuery}");
                await using var stream = await response.Content.ReadAsStreamAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };

                if (response.IsSuccessStatusCode)
                    return await JsonSerializer.DeserializeAsync<List<LocationMapping>>(stream, options);

                var error = await JsonSerializer.DeserializeAsync<ProblemDetails>(stream, options) ??
                    ProblemDetailsBuilder.Build(response.ReasonPhrase, response.StatusCode);

                return Result.Failure<List<LocationMapping>, ProblemDetails>(error);
            }
            catch (Exception ex)
            {
                _logger.LogMapperClientException(ex);
                return ProblemDetailsBuilder.Fail<List<LocationMapping>>(ex.Message);
            }
        }
        
        
        public async Task<List<SlimAccommodation>> GetAccommodations(List<string> htIds, string languageCode)
        {
            if (htIds.Any())
            {
                var client = _clientFactory.CreateClient(HttpClientNames.MapperApi);
                try
                {
                    var htIdQuery = string.Join("&", htIds.Select(h => $"htIds={h}"));
                    using var response = await client.GetAsync($"api/1.0/accommodations?{htIdQuery}");
                    await using var stream = await response.Content.ReadAsStreamAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    };

                    if (response.IsSuccessStatusCode)
                        return await JsonSerializer.DeserializeAsync<List<SlimAccommodation>>(stream, options);
                }
                catch (Exception ex)
                {
                    _logger.LogMapperClientException(ex);
                }
            }

            return new List<SlimAccommodation>();
        }


        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<AccommodationMapperClient> _logger;
    }
}