using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.DirectApi.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using JsonException = System.Text.Json.JsonException;

namespace HappyTravel.Edo.DirectApi.Services
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
            return await Send<List<Accommodation>>(endpoint, languageCode);
        }


        public Task<Result<Accommodation>> GetAccommodationById(string accommodationId, string languageCode)
        {
            var endpoint = $"api/1.0/accommodations/{accommodationId}";
            return Send<Accommodation>(endpoint, languageCode);
        }


        private async Task<Result<T>> Send<T>(string endpoint, string languageCode)
        {
            using var client = _httpClientFactory.CreateClient(HttpClientNames.MapperApi);
            client.DefaultRequestHeaders.Add("Accept-Language", languageCode);
            
            try
            {
                var response = await client.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                    return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());

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
                
                return Result.Failure<T>(error);
            }
            catch (Exception ex)
            {
                return Result.Failure<T>(ex.Message);
            }
        }


        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAccommodationBookingSettingsService _accommodationBookingSettingsService;
        private readonly IAgentContextService _agentContextService;
    }
}