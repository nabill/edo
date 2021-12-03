using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.DirectApi.Models.Static;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.DirectApi.Services.Static
{
    public class AccommodationService
    {
        public AccommodationService(IHttpClientFactory httpClientFactory, IAccommodationBookingSettingsService accommodationBookingSettingsService, 
            IAgentContextService agentContextService)
        {
            _httpClientFactory = httpClientFactory;
            _accommodationBookingSettingsService = accommodationBookingSettingsService;
            _agentContextService = agentContextService;
        }


        public async Task<Result<List<Accommodation>>> GetAccommodationList(int top, int skip, string languageCode)
        {
            var agent = await _agentContextService.GetAgent();
            var searchSettings = await _accommodationBookingSettingsService.Get(agent);
            var endpoint = $"api/1.0/accommodations?top={top}&skip={skip}&suppliers={string.Join("&suppliers=", searchSettings.EnabledConnectors)}";
            var (_, isFailure, accommodations, error) = await Send<List<MapperContracts.Public.Accommodations.Accommodation>?>(endpoint, languageCode);
            
            if (isFailure)
                return Result.Failure<List<Accommodation>>(error);
            
            return accommodations?.ToDirectApiModels() ?? Result.Failure<List<Accommodation>>("Failed to get accommodations");
        }


        public async Task<Result<Accommodation>> GetAccommodationById(string accommodationId, string languageCode)
        {
            var endpoint = $"api/1.0/accommodations/{accommodationId}";
            var (_, isFailure, accommodation, error) = await Send<MapperContracts.Public.Accommodations.Accommodation?>(endpoint, languageCode);

            if (isFailure)
                return Result.Failure<Accommodation>(error);
            
            return accommodation?.ToDirectApiModel() ?? Result.Failure<Accommodation>("Failed to get accommodation");
        }


        private async Task<Result<T?>> Send<T>(string endpoint, string languageCode)
        {
            using var client = _httpClientFactory.CreateClient(HttpClientNames.MapperApi);
            client.DefaultRequestHeaders.Add("Accept-Language", languageCode);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                Converters = { new JsonStringEnumConverter() }
            };
            
            try
            {
                var response = await client.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                    return JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync(), options);

                string error;

                try
                {
                    var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
                    error = problemDetails?.Detail ?? string.Empty;
                }
                catch (JsonException)
                {
                    error = await response.Content.ReadAsStringAsync();
                }
                
                return Result.Failure<T?>(error);
            }
            catch (Exception ex)
            {
                return Result.Failure<T?>(ex.Message);
            }
        }


        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAccommodationBookingSettingsService _accommodationBookingSettingsService;
        private readonly IAgentContextService _agentContextService;
    }
}