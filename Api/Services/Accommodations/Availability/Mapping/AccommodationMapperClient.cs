using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.MapperContracts.Internal.Mappings;
using HappyTravel.MapperContracts.Public.Accommodations;
using HappyTravel.SuppliersCatalog;
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
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                    Converters = { new JsonStringEnumConverter() }
                };

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<List<LocationMapping>>(options);

                var error = await response.Content.ReadFromJsonAsync<ProblemDetails>(options) ??
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
                    var requestContent = new StringContent(JsonConvert.SerializeObject(htIds), Encoding.UTF8, "application/json");
                    using var response = await client.PostAsync("api/1.0/accommodations-list", requestContent);
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                        Converters = { new JsonStringEnumConverter() }
                    };

                    if (response.IsSuccessStatusCode)
                    {
                        var results = await response.Content.ReadFromJsonAsync<List<SlimAccommodation>>(options);
                        if (results is null)
                        {
                            _logger.LogError("Request for {HtIds} returned null", htIds);
                            return new List<SlimAccommodation>();
                        }
                            
                        else if (results.Count != htIds.Count)
                        {
                            _logger.LogWarning("Returned {ActualCount} accommodations while expected {ExpectedCount}", results.Count, htIds.Count);
                        }

                        return results;
                    }
                    else
                    {
                        _logger.LogError("Request to mapper failed: {Message}:{StatusCode}", await response.Content.ReadAsStringAsync(), response.StatusCode);
                    }
                        
                }
                catch (Exception ex)
                {
                    _logger.LogMapperClientException(ex);
                }
            }

            return new List<SlimAccommodation>();
        }


        public async Task<Result<Accommodation, ProblemDetails>> GetAccommodation(string htId, string languageCode)
        {
            var client = _clientFactory.CreateClient(HttpClientNames.MapperApi);
            try
            {
                using var response = await client.GetAsync($"api/1.0/accommodations/{htId}");
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                    Converters = { new JsonStringEnumConverter() }
                };

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<Accommodation>(options);
                
                var error = await response.Content.ReadFromJsonAsync<ProblemDetails>(options) ??
                    ProblemDetailsBuilder.Build(response.ReasonPhrase, response.StatusCode);

                return Result.Failure<Accommodation, ProblemDetails>(error);
            }
            catch (Exception ex)
            {
                _logger.LogMapperClientException(ex);
                return ProblemDetailsBuilder.Fail<Accommodation>(ex.Message);
            }
        }
        
        
        public async Task<Result<Accommodation, ProblemDetails>> GetAccommodation(Suppliers supplier, string accommodationId, string languageCode)
        {
            var client = _clientFactory.CreateClient(HttpClientNames.MapperApi);
            try
            {
                using var response = await client.GetAsync($"api/1.0/suppliers/{supplier}/accommodations/{accommodationId}");
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                    Converters = { new JsonStringEnumConverter() }
                };

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<Accommodation>(options);
                
                var error = await response.Content.ReadFromJsonAsync<ProblemDetails>(options) ??
                    ProblemDetailsBuilder.Build(response.ReasonPhrase, response.StatusCode);

                return Result.Failure<Accommodation, ProblemDetails>(error);
            }
            catch (Exception ex)
            {
                _logger.LogMapperClientException(ex);
                return ProblemDetailsBuilder.Fail<Accommodation>(ex.Message);
            }
        }


        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<AccommodationMapperClient> _logger;
    }
}