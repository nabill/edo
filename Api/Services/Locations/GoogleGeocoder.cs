using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.Edo.Api.Models.Locations.Google;
using HappyTravel.Edo.Common.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Prediction = HappyTravel.Edo.Api.Models.Locations.Prediction;

namespace HappyTravel.Edo.Api.Services.Locations
{
    public class GoogleGeoCoder : IGeoCoder
    {
        public GoogleGeoCoder(ILoggerFactory loggerFactory, IHttpClientFactory clientFactory, IOptions<GoogleOptions> options)
        {
            _clientFactory = clientFactory;
            _logger = loggerFactory.CreateLogger<GoogleGeoCoder>();
            _options = options.Value;

            _serializer = new JsonSerializer();
        }


        public async Task<Result<Location>> GetLocation(string locationId, string sessionId, LocationTypes type)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                return Result.Fail<Location>(
                    "A session must be provided. The session begins when the user starts typing a query, and concludes when they select a place. " +
                    "Each session can have multiple queries, followed by one place selection. Once a session has concluded, the token is no longer valid; " +
                    "your app must generate a fresh token for each session.");

            var url = $"place/details/json?key={_options.ApiKey}&placeid={locationId}&sessiontoken={sessionId}" +
                "&language=en&fields=address_component,adr_address,formatted_address,geometry,name,place_id,type,vicinity";

            var maybePlaceContainer = await GetResponseContent<PlaceContainer>(url);
            if (maybePlaceContainer.HasNoValue)
                return Result.Fail<Location>("A network error has been occurred. Please retry your request after several seconds.");

            var place = maybePlaceContainer.Value.Place;
            if (place.Equals(default))
                return Result.Fail<Location>("A network error has been occurred. Please retry your request after several seconds.");

            var viewPortDistance = CalculateDistance(place.Geometry.Viewport.NorthEast.Longitude, place.Geometry.Viewport.NorthEast.Latitude,
                place.Geometry.Viewport.SouthWest.Longitude, place.Geometry.Viewport.SouthWest.Latitude);
            var distance = (int) viewPortDistance / 2;

            var locality = place.Components.FirstOrDefault(c => c.Types.Contains("locality")).Name ?? string.Empty;
            var country = place.Components.FirstOrDefault(c => c.Types.Contains("country")).Name ?? string.Empty;

            return Result.Ok(new Location(place.Name, locality, country, place.Geometry.Location, distance, PredictionSources.Google, type));
        }


        public async ValueTask<Result<List<Prediction>>> GetLocationPredictions(string query, string sessionId, string languageCode)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                return Result.Fail<List<Prediction>>(
                    "A session must be provided. The session begins when the user starts typing a query, and concludes when they select a place. " +
                    "Each session can have multiple queries, followed by one place selection. Once a session has concluded, the token is no longer valid; " +
                    "your app must generate a fresh token for each session.");

            if (string.IsNullOrWhiteSpace(query) || query.Length < SearchThreshold)
                return Result.Ok(new List<Prediction>());

            var url = $"place/autocomplete/json?input={query}&key={_options.ApiKey}&session={sessionId}";
            if (!string.IsNullOrWhiteSpace(languageCode))
                url += "&language=" + languageCode;

            var maybeContainer = await GetResponseContent<PredictionsContainer>(url);
            if (maybeContainer.HasNoValue)
                return Result.Fail<List<Prediction>>("A network error has been occurred. Please retry your request after several seconds.");

            return maybeContainer.Value.Predictions.Any()
                ? BuildPredictions(maybeContainer.Value.Predictions)
                : Result.Ok(new List<Prediction>());
        }


        private static Result<List<Prediction>> BuildPredictions(in List<Models.Locations.Google.Prediction> googlePredictions)
        {
            var results = new List<Prediction>();
            foreach (var prediction in googlePredictions)
            {
                var type = GetLocationType(prediction.Types);
                if (type == LocationTypes.Unknown)
                    continue;

                results.Add(new Prediction(prediction.Id, PredictionSources.Google, prediction.Matches, type, prediction.Description));
            }

            return Result.Ok(results);
        }


        private static double CalculateDistance(double longitude1, double latitude1, double longitude2, double latitude2)
        {
            var latitudeDelta = ToRadians(latitude2 - latitude1);
            var longitudeDelta = ToRadians(longitude2 - longitude1);

            latitude1 = ToRadians(latitude1);
            latitude2 = ToRadians(latitude2);

            // Haversine formula:
            // a = sin²(Δφ/2) + cos φ1 ⋅ cos φ2 ⋅ sin²(Δλ/2)
            // c = 2 ⋅ atan2( √a, √(1−a) )
            // d = R ⋅ c
            var halfChordLengthSquare = Math.Sin(latitudeDelta / 2) * Math.Sin(latitudeDelta / 2) +
                Math.Sin(longitudeDelta / 2) * Math.Sin(longitudeDelta / 2) * Math.Cos(latitude1) * Math.Cos(latitude2);
            var angularDistance = 2 * Math.Atan2(Math.Sqrt(halfChordLengthSquare), Math.Sqrt(1 - halfChordLengthSquare));

            return EarthRadiusInKm * angularDistance * 1000;

            double ToRadians(double target) => target * Math.PI / 180;
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
                {
                    return LocationTypes.Location;
                }

                if (type.Equals("airport", StringComparison.OrdinalIgnoreCase) ||
                    type.Equals("subway_station", StringComparison.OrdinalIgnoreCase) ||
                    type.Equals("train_station", StringComparison.OrdinalIgnoreCase) ||
                    type.Equals("transit_station", StringComparison.OrdinalIgnoreCase))
                {
                    return LocationTypes.Destination;
                }

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
                {
                    return LocationTypes.Landmark;
                }
            }

            return LocationTypes.Unknown;
        }


        private async Task<Maybe<T>> GetResponseContent<T>(string url) where T : GoogleResponse
        {
            try
            {
                using (var client = _clientFactory.CreateClient(HttpClientNames.GoogleMaps))
                using (var response = await client.GetAsync(url))
                {
                    if (!response.IsSuccessStatusCode)
                        return Maybe<T>.None;

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var streamReader = new StreamReader(stream))
                    using (var jsonTextReader = new JsonTextReader(streamReader))
                    {
                        var result = _serializer.Deserialize<T>(jsonTextReader);
                        //TODO: full code list https://developers.google.com/places/web-service/autocomplete#place_autocomplete_status_codes
                        if (result.Status.ToUpperInvariant() != "OK")
                            return Maybe<T>.None;

                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogGeocoderException(ex);
                return default;
            }
        }

        
        private const int EarthRadiusInKm = 6371;
        private const int SearchThreshold = 3;

        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<GoogleGeoCoder> _logger;
        private readonly GoogleOptions _options;
        private readonly JsonSerializer _serializer;
    }
}