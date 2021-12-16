using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices.Models.Mapper;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.MapperContracts.Public.Accommodations.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.AdministratorServices.Mapper.AccommodationManagementServices
{
    public class MapperManagementClient : IMapperManagementClient
    {
        public MapperManagementClient(IHttpClientFactory clientFactory, ILogger<MapperManagementClient> logger)
        {
            _httpClient = clientFactory.CreateClient(HttpClientNames.MapperManagement);
            _logger = logger;
        }
        
        
        public async Task<Result<Unit, ProblemDetails>> MergeAccommodations(AccommodationsMergeRequest accommodationsMergeRequest, CancellationToken cancellationToken = default)
        {
            using var requestContent = new StringContent(JsonSerializer.Serialize(accommodationsMergeRequest), Encoding.UTF8, "application/json");
            var requestUri = "api/1.0/AccommodationsManagement/accommodations/merge";
            
            return await Post(requestUri, requestContent, cancellationToken: cancellationToken);
        }


        public async Task<Result<Unit, ProblemDetails>> DeactivateAccommodations(DeactivateAccommodationsRequest request, AccommodationDeactivationReasons deactivationReason, CancellationToken cancellationToken)
        {
            using var requestContent = new StringContent(JsonSerializer.Serialize(new {request.HtAccommodationIds, reason = deactivationReason}), Encoding.UTF8, "application/json");
            var requestUri = "api/1.0/AccommodationsManagement/accommodations/deactivate";
            
            return await Post(requestUri, requestContent, cancellationToken: cancellationToken);
        }

        
        public async Task<Result<Unit, ProblemDetails>> DeactivateAccommodationManually(string htAccommodationId, string DeactivationReasonDescription, CancellationToken cancellationToken)
        {
            using var requestContent = new StringContent($@"""{nameof(DeactivationReasonDescription)}"" = ""{DeactivationReasonDescription}""", Encoding.UTF8, "application/json");
            var requestUri = $"api/1.0/AccommodationsManagement/accommodations/{htAccommodationId}/deactivate-manually";
            
            return await Post(requestUri, requestContent, cancellationToken: cancellationToken);
        }
        

        public async Task<Result<Unit, ProblemDetails>> RemoveSupplier(string htAccommodationId, RemoveSupplierRequest request, CancellationToken cancellationToken = default)
        {
            using var requestContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var requestUri = $"api/1.0/AccommodationsManagement/accommodations/{htAccommodationId}/suppliers/remove";
            
            return await Post(requestUri, requestContent, cancellationToken: cancellationToken);
        }


        public Task<Result<DetailedAccommodation, ProblemDetails>> GetDetailedAccommodationData(string accommodationHtId, string languageCode, CancellationToken cancellationToken)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"api/1.0/admin/accommodations/{accommodationHtId}/detailed-data");
            
            return Send<DetailedAccommodation>(requestMessage, languageCode, cancellationToken);
        }


        public async Task<Result<List<SlimAccommodationData>, ProblemDetails>> SearchAccommodations(AccommodationSearchRequest request, CancellationToken cancellationToken)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"api/1.0/admin/accommodations/search")
            {
                Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
            };
  
            return await Send<List<SlimAccommodationData>>(requestMessage, cancellationToken: cancellationToken);
        }


        public Task<Result<Dictionary<int, string>, ProblemDetails>> GetDeactivationReasonTypes(CancellationToken cancellationToken)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"api/1.0/admin/accommodations/deactivation-reason-types");
            
            return Send<Dictionary<int, string>>(requestMessage, cancellationToken: cancellationToken);
        }


        public Task<Result<Dictionary<int, string>, ProblemDetails>> GetRatingTypes(CancellationToken cancellationToken)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"api/1.0/admin/accommodations/rating-types");
            
            return Send<Dictionary<int, string>>(requestMessage, cancellationToken: cancellationToken);
        }


        public Task<Result<List<CountryData>, ProblemDetails>> SearchCountries(string query, string languageCode, CancellationToken cancellationToken)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"api/1.0/admin/locations/countries/search?{nameof(query)}={query}");
            
            return Send<List<CountryData>>(requestMessage, languageCode, cancellationToken);
        }


        public Task<Result<List<LocalityData>, ProblemDetails>> SearchLocalities(int countryId, string query, string languageCode, CancellationToken cancellationToken)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"api/1.0/admin/locations/countries/{countryId}/localities/search?{nameof(query)}={query}");
            
            return Send<List<LocalityData>>(requestMessage, languageCode, cancellationToken);
        }
        
        
        public Task<Result<Dictionary<int, string>, ProblemDetails>> GetSuppliers(CancellationToken cancellationToken)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"api/1.0/admin/suppliers");
            
            return Send<Dictionary<int, string>>(requestMessage, cancellationToken: cancellationToken);
        }
        

        private async Task<Result<TResponse, ProblemDetails>> Send<TResponse>(HttpRequestMessage requestMessage,
            string languageCode = LocalizationHelper.DefaultLanguageCode, CancellationToken cancellationToken = default)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Add("Accept-Language", languageCode);
                using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

                if (response.IsSuccessStatusCode)
                    return (await response.Content.ReadFromJsonAsync<TResponse>(JsonSerializerOptions, cancellationToken))!;

                return Result.Failure<TResponse, ProblemDetails>( await GetProblemDetails(response, cancellationToken));
            }
            catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
            {
                _logger.LogMapperManagementClientRequestTimeout(ex);
                
                return ProblemDetailsBuilder.Build("Request failure");
            }
            catch (Exception ex)
            {
                _logger.LogMapperManagementClientException(ex);
                
                return ProblemDetailsBuilder.Build(ex.Message);
            }
        }
        
        
        private async Task<Result<Unit, ProblemDetails>> Post(string requestUri, HttpContent content, string languageCode = LocalizationHelper.DefaultLanguageCode, CancellationToken cancellationToken = default)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Add("Accept-Language", languageCode);
                using var response = await _httpClient.PostAsync(requestUri, content, cancellationToken);

                if (response.IsSuccessStatusCode)
                    return Result.Success<Unit, ProblemDetails>(Unit.Instance);

                return Result.Failure<Unit, ProblemDetails>(await GetProblemDetails(response, cancellationToken));
            }
            catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
            {
                _logger.LogMapperManagementClientRequestTimeout(ex);
                return ProblemDetailsBuilder.Build("Request failure");
            }
            catch (Exception ex)
            {
                _logger.LogMapperManagementClientException(ex);
                return ProblemDetailsBuilder.Build(ex.Message);
            }
        }
        
        
        private async Task<ProblemDetails> GetProblemDetails(HttpResponseMessage responseMessage, CancellationToken cancellationToken)
        {
            if (responseMessage.IsSuccessStatusCode)
                return new ProblemDetails();
            
            ProblemDetails error;
            try
            {
                error = await responseMessage.Content.ReadFromJsonAsync<ProblemDetails>(JsonSerializerOptions, cancellationToken);
            }
            catch (JsonException)
            {
                var responseBody = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogMapperManagementClientUnexpectedResponse(responseMessage.StatusCode, responseMessage.RequestMessage?.RequestUri, responseBody);
                error = ProblemDetailsBuilder.Build(responseMessage.ReasonPhrase, responseMessage.StatusCode);
            }

            return error;
        }
        
        
        private static readonly JsonSerializerOptions JsonSerializerOptions = new () 
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            Converters = { new JsonStringEnumConverter() }
        };
        
        
        private readonly HttpClient _httpClient;
        private readonly ILogger<MapperManagementClient> _logger;
    }
}