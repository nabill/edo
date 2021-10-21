using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.MapperContracts.Internal.Mappings;
using HappyTravel.MapperContracts.Public.Accommodations;
using HappyTravel.MapperContracts.Public.Accommodations.Enums;
using HappyTravel.SuppliersCatalog;
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
            _options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                Converters = { new JsonStringEnumConverter() }
            };
        }
        
        
        public async Task<Result<List<LocationMapping>, ProblemDetails>> GetMappings(List<string> htIds, string languageCode, CancellationToken cancellationToken = default)
        {
            if (!htIds.Any())
                return ProblemDetailsBuilder.Fail<List<LocationMapping>>("Could not get mapping for an empty ids list");
                    
            var client = _clientFactory.CreateClient(HttpClientNames.MapperApi);
            try
            {
                var htIdQuery = string.Join("&", htIds.Select(h => $"htIds={h}"));
                using var response = await client.GetAsync($"api/1.0/location-mappings?{htIdQuery}", cancellationToken);

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<List<LocationMapping>>(_options, cancellationToken);
                
                ProblemDetails error;

                try
                {
                    error = await response.Content.ReadFromJsonAsync<ProblemDetails>(_options, cancellationToken);
                }
                catch (JsonException)
                {
                    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogMapperClientUnexpectedResponse(response.StatusCode, response.RequestMessage?.RequestUri, responseBody);
                    error = ProblemDetailsBuilder.Build(response.ReasonPhrase, response.StatusCode);
                }

                return Result.Failure<List<LocationMapping>, ProblemDetails>(error);
            }
            // This is timeout exception
            catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
            {
                _logger.LogMapperClientRequestTimeout(ex);
                return ProblemDetailsBuilder.Fail<List<LocationMapping>>("Static data request failure");
            }
            catch (Exception ex)
            {
                _logger.LogMapperClientException(ex);
                return ProblemDetailsBuilder.Fail<List<LocationMapping>>(ex.Message);
            }
        }
        
        
        public async Task<Result<SlimLocationDescription, ProblemDetails>> GetSlimDescription(string htId, CancellationToken cancellationToken = default)
        {
            using var client = _clientFactory.CreateClient(HttpClientNames.MapperApi);
            try
            {
                using var response = await client.GetAsync($"api/1.0/locations/{htId}/slim-description", cancellationToken);

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<SlimLocationDescription>(cancellationToken: cancellationToken);

                ProblemDetails error;

                try
                {
                    error = await response.Content.ReadFromJsonAsync<ProblemDetails>(_options, cancellationToken);
                }
                catch (JsonException)
                {
                    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogMapperClientUnexpectedResponse(response.StatusCode, response.RequestMessage?.RequestUri, responseBody);
                    error = ProblemDetailsBuilder.Build(response.ReasonPhrase, response.StatusCode);
                }

                return Result.Failure<SlimLocationDescription, ProblemDetails>(error);
            }
            catch (Exception ex)
            {
                _logger.LogMapperClientException(ex);
                return ProblemDetailsBuilder.Fail<SlimLocationDescription>(ex.Message);
            }
        }
        
        
        public async Task<List<SlimAccommodation>> GetAccommodations(List<string> htIds, string languageCode, CancellationToken cancellationToken = default)
        {
            if (htIds.Any())
            {
                var client = _clientFactory.CreateClient(HttpClientNames.MapperApi);
                try
                {
                    var requestContent = new StringContent(JsonSerializer.Serialize(htIds), Encoding.UTF8, "application/json");
                    using var response = await client.PostAsync("api/1.0/accommodations-list", requestContent, cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        var results = await response.Content.ReadFromJsonAsync<List<SlimAccommodation>>(_options, cancellationToken);
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
                        _logger.LogMapperClientErrorResponse(await response.Content.ReadAsStringAsync(cancellationToken), (int)response.StatusCode, htIds.ToArray());
                    }
                        
                }
                catch (Exception ex)
                {
                    _logger.LogMapperClientException(ex);
                }
            }

            return new List<SlimAccommodation>();
        }


        public async Task<Result<Accommodation, ProblemDetails>> GetAccommodation(string htId, string languageCode, CancellationToken cancellationToken = default)
        {
            var client = _clientFactory.CreateClient(HttpClientNames.MapperApi);
            try
            {
                using var response = await client.GetAsync($"api/1.0/accommodations/{htId}", cancellationToken);

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<Accommodation>(_options, cancellationToken);
                
                var error = await response.Content.ReadFromJsonAsync<ProblemDetails>(_options, cancellationToken) ??
                    ProblemDetailsBuilder.Build(response.ReasonPhrase, response.StatusCode);

                return Result.Failure<Accommodation, ProblemDetails>(error);
            }
            catch (Exception ex)
            {
                _logger.LogMapperClientException(ex);
                return ProblemDetailsBuilder.Fail<Accommodation>(ex.Message);
            }
        }
        
        
        public async Task<Result<Accommodation, ProblemDetails>> GetAccommodation(Suppliers supplier, string accommodationId, string languageCode, CancellationToken cancellationToken = default)
        {
            var client = _clientFactory.CreateClient(HttpClientNames.MapperApi);
            try
            {
                using var response = await client.GetAsync($"api/1.0/suppliers/{supplier}/accommodations/{accommodationId}", cancellationToken);

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<Accommodation>(_options, cancellationToken);
                
                var error = await response.Content.ReadFromJsonAsync<ProblemDetails>(_options, cancellationToken) ??
                    ProblemDetailsBuilder.Build(response.ReasonPhrase, response.StatusCode);

                return Result.Failure<Accommodation, ProblemDetails>(error);
            }
            catch (Exception ex)
            {
                _logger.LogMapperClientException(ex);
                return ProblemDetailsBuilder.Fail<Accommodation>(ex.Message);
            }
        }


        public async Task<Result<List<string>, ProblemDetails>> GetAccommodationEmails(string htId, CancellationToken cancellationToken = default)
        {
            var client = _clientFactory.CreateClient(HttpClientNames.MapperApi);
            try
            {
                using var response = await client.GetAsync($"api/1.0/accommodations/{htId}/email-addresses", cancellationToken);

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<List<string>>(_options, cancellationToken);

                var error = await response.Content.ReadFromJsonAsync<ProblemDetails>(_options, cancellationToken) ??
                    ProblemDetailsBuilder.Build(response.ReasonPhrase, response.StatusCode);

                return Result.Failure<List<string>, ProblemDetails>(error);
            }
            catch (Exception ex)
            {
                _logger.LogMapperClientException(ex);
                return ProblemDetailsBuilder.Fail<List<string>>(ex.Message);
            }
        }
        
        
        public async Task<List<string>> FilterHtIdsByRating(List<string> htIds, List<AccommodationRatings> ratings, CancellationToken cancellationToken = default)
        {
            using var client = _clientFactory.CreateClient(HttpClientNames.MapperApi);
            try
            {
                var ratingsQuery = string.Join("&", ratings.Select(r => $"ratings={r}"));
                using var response = await client.PostAsJsonAsync($"api/1.0/accommodations/filtered-by-rating?{ratingsQuery}", htIds, cancellationToken);

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<List<string>>(cancellationToken: cancellationToken);

                _logger.LogMapperClientUnexpectedResponse(response.StatusCode, response.RequestMessage?.RequestUri, await response.Content.ReadAsStringAsync(cancellationToken));
                return new List<string>(0);
            }
            catch (Exception ex)
            {
                _logger.LogMapperClientException(ex);
                return new List<string>(0);
            }
        }


        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<AccommodationMapperClient> _logger;
        private readonly JsonSerializerOptions _options;
    }
}