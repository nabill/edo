using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.Edo.Api.Models.Locations.Google;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Prediction = HappyTravel.Edo.Api.Models.Locations.Prediction;

namespace HappyTravel.Edo.Api.Services.Locations
{
    public class GoogleGeocoder : IGeocoder
    {
        public GoogleGeocoder(IHttpClientFactory clientFactory, IOptions<GoogleOptions> options)
        {
            _clientFactory = clientFactory;
            _options = options.Value;
        }


        public async ValueTask<Result<List<Prediction>>> GetLocationPredictions(string query, string session, string languageCode)
        {
            if (string.IsNullOrWhiteSpace(session))
                return Result.Fail<List<Prediction>>(
                    "A session must be provided. The session begins when the user starts typing a query, and concludes when they select a place. " +
                    "Each session can have multiple queries, followed by one place selection. Once a session has concluded, the token is no longer valid; " +
                    "your app must generate a fresh token for each session.");

            if (string.IsNullOrWhiteSpace(query) || query.Length < SearchThreshold)
                return Result.Ok(new List<Prediction>());

            var url = $"place/autocomplete/json?input={query}&key={_options.ApiKey}&session={session}";
            if (!string.IsNullOrWhiteSpace(languageCode))
                url += "&language=" + languageCode;

            var container = await GetResponseContent<PredictionsContainer>(url);
            return container.Predictions.Any() 
                ? BuildPredictions(container.Predictions) 
                : Result.Ok(new List<Prediction>());
        }


        private static Result<List<Prediction>> BuildPredictions(in List<Models.Locations.Google.Prediction> googlePredictions)
        {
            var results = new List<Prediction>();
            foreach (var prediction in googlePredictions)
            {
                var type = GetLocationType(prediction.Types);
                if (type == LocationTypes.Irrelevant)
                    continue;

                results.Add(new Prediction(prediction.Id, prediction.Matches, type, prediction.Description));
            }

            return Result.Ok(results);
        }


        private static LocationTypes GetLocationType(List<string> types)
        {
            foreach (ReadOnlySpan<char> type in types)
            {
                if (type.Equals("administrative_area_level_1", StringComparison.OrdinalIgnoreCase) ||
                    type.Equals("administrative_area_level_2", StringComparison.OrdinalIgnoreCase) ||
                    type.Equals("administrative_area_level_3", StringComparison.OrdinalIgnoreCase) ||
                    type.Equals("administrative_area_level_4", StringComparison.OrdinalIgnoreCase) ||
                    type.Equals("administrative_area_level_5", StringComparison.OrdinalIgnoreCase) ||
                    type.Equals("colloquial_area", StringComparison.OrdinalIgnoreCase) || 
                    type.Equals("country", StringComparison.OrdinalIgnoreCase) ||
                    type.Equals("geocode", StringComparison.OrdinalIgnoreCase) ||
                    type.Equals("locality", StringComparison.OrdinalIgnoreCase) ||
                    type.Equals("natural_feature", StringComparison.OrdinalIgnoreCase) ||
                    type.Equals("neighborhood", StringComparison.OrdinalIgnoreCase) ||
                    type.Equals("place_of_worship", StringComparison.OrdinalIgnoreCase) ||
                    type.Equals("political", StringComparison.OrdinalIgnoreCase) ||
                    type.Equals("postal_code", StringComparison.OrdinalIgnoreCase) ||
                    type.Equals("street_address", StringComparison.OrdinalIgnoreCase) ||
                    type.Equals("sublocality_level_1", StringComparison.OrdinalIgnoreCase))
                    return LocationTypes.Location;

                if (type.Equals("airport", StringComparison.OrdinalIgnoreCase) || 
                    type.Equals("subway_station", StringComparison.OrdinalIgnoreCase) || 
                    type.Equals("train_station", StringComparison.OrdinalIgnoreCase) || 
                    type.Equals("transit_station", StringComparison.OrdinalIgnoreCase))
                    return LocationTypes.Destination;

                if (type.Equals("amusement_park", StringComparison.OrdinalIgnoreCase) || 
                    type.Equals("aquarium", StringComparison.OrdinalIgnoreCase) || 
                    type.Equals("art_gallery", StringComparison.OrdinalIgnoreCase) || 
                    type.Equals("campground", StringComparison.OrdinalIgnoreCase) || 
                    type.Equals("church", StringComparison.OrdinalIgnoreCase) ||
                    type.Equals("hindu_temple", StringComparison.OrdinalIgnoreCase) || 
                    type.Equals("mosque", StringComparison.OrdinalIgnoreCase) || 
                    type.Equals("museum", StringComparison.OrdinalIgnoreCase) || 
                    type.Equals("point_of_interest", StringComparison.OrdinalIgnoreCase) || 
                    type.Equals("stadium", StringComparison.OrdinalIgnoreCase) || 
                    type.Equals("zoo", StringComparison.OrdinalIgnoreCase))
                    return LocationTypes.Landmark;
            }

            return LocationTypes.Irrelevant;
        }


        private async Task<T> GetResponseContent<T>(string url) where T : GoogleResponse
        {
            try
            {
                using (var client = _clientFactory.CreateClient("google-maps"))
                using (var response = await client.GetAsync(url))
                {
                    if (!response.IsSuccessStatusCode)
                        return default;

                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<T>(json);
                    //TODO: full code list https://developers.google.com/places/web-service/autocomplete#place_autocomplete_status_codes
                    if (result.Status.ToUpperInvariant() != "OK")
                        return default;

                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return default;
            }
        }


        private const int SearchThreshold = 3;

        private readonly IHttpClientFactory _clientFactory;
        private readonly GoogleOptions _options;
    }
}
