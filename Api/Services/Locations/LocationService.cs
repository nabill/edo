using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.Edo.Api.Models.Locations.Google;
using HappyTravel.Edo.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Prediction = HappyTravel.Edo.Api.Models.Locations.Prediction;

namespace HappyTravel.Edo.Api.Services.Locations
{
    public class LocationService : ILocationService
    {
        public LocationService(EdoContext context, IMemoryFlow flow, IGeoCoder geoCoder)
        {
            _context = context;
            _flow = flow;
            _geoCoder = geoCoder;
        }


        public async ValueTask<Result<Location, ProblemDetails>> Get(SearchLocation searchLocation)
        {
            if (string.IsNullOrWhiteSpace(searchLocation.PredictionResult.Id))
                return Result.Ok<Location, ProblemDetails>(new Location(string.Empty, string.Empty, string.Empty, searchLocation.Coordinates,
                    searchLocation.DistanceInMeters, LocationTypes.Unknown));

            if (searchLocation.PredictionResult.Type == LocationTypes.Unknown)
                return Result.Fail<Location, ProblemDetails>(
                    ProblemDetailsBuilder.Build("Invalid prediction type. It looks like a prediction type was not specified in the request."));

            if (searchLocation.PredictionResult.Source == PredictionSources.Google)
                return await GetLocationFromGeoCoder(searchLocation);

            //TODO: implement other sources
            return Result.Ok<Location, ProblemDetails>(default);
        }


        public ValueTask<List<Country>> GetCountries(string query, string languageCode)
        {
            if (query?.Length < 2)
                return GetFullCountryList(languageCode);

            return _flow.GetOrSetAsync(_flow.BuildKey(nameof(LocationService), CountriesKeyBase, languageCode, query), async () =>
            {
                var results = await _context.Countries
                    .Where(c => EF.Functions.ILike(c.Code, query) || EF.Functions.ILike(EdoContext.JsonbToString(c.Names), $"%{query}%"))
                    .Select(c => new Country(c.Code, JsonConvert.DeserializeObject<Dictionary<string, string>>(c.Names), c.RegionId))
                    .ToListAsync();

                if (string.IsNullOrWhiteSpace(languageCode))
                    return results;

                return results.Select(r =>
                {
                    var name = LocalizationHelper.GetValue(r.Names, languageCode);
                    return new Country(r.Code, new Dictionary<string, string> {{languageCode, name}}, r.RegionId);
                }).ToList();
            }, TimeSpan.FromDays(1));
        }


        public ValueTask<Result<List<Prediction>, ProblemDetails>> GetPredictions(string query, string session, string languageCode)
        {
            query = query.ToLowerInvariant();

            return _flow.GetOrSetAsync(_flow.BuildKey(nameof(LocationService), PredictionsKeyBase, languageCode, query), async () =>
            {
                var localResults = new List<Prediction>();
                if (localResults.Count >= DesirableNumberOfLocalPredictions)
                    return Result.Ok<List<Prediction>, ProblemDetails>(localResults);

                var (_, isFailure, predictions, error) = await _geoCoder.GetPlacePredictions(query, session, languageCode);
                if (isFailure)
                    return Result.Fail<List<Prediction>, ProblemDetails>(new ProblemDetails
                    {
                        Detail = error,
                        Status = (int) HttpStatusCode.BadRequest
                    });

                localResults.AddRange(predictions);

                return Result.Ok<List<Prediction>, ProblemDetails>(localResults);
            }, TimeSpan.FromDays(1));
        }


        public ValueTask<List<Region>> GetRegions(string languageCode)
            => _flow.GetOrSetAsync(_flow.BuildKey(nameof(LocationService), RegionsKeyBase, languageCode), async () =>
            {
                var isLanguageCodeEmpty = string.IsNullOrWhiteSpace(languageCode);
                return (await _context.Regions.ToListAsync())
                    .Select(r =>
                    {
                        var storedNames = JsonConvert.DeserializeObject<Dictionary<string, string>>(r.Names);
                        if (isLanguageCodeEmpty)
                            return new Region(r.Id, storedNames);

                        var name = LocalizationHelper.GetValue(storedNames, languageCode);
                        return new Region(r.Id, new Dictionary<string, string> {{languageCode, name}});
                    }).ToList();
            }, TimeSpan.FromDays(1));


        public ValueTask<List<Country>> GetFullCountryList(string languageCode)
            => _flow.GetOrSetAsync(_flow.BuildKey(nameof(LocationService), CountriesKeyBase, languageCode), async () =>
            {
                var isLanguageCodeEmpty = string.IsNullOrWhiteSpace(languageCode);
                return (await _context.Countries.ToListAsync())
                    .Select(c =>
                    {
                        var storedNames = JsonConvert.DeserializeObject<Dictionary<string, string>>(c.Names);
                        if (isLanguageCodeEmpty)
                            return new Country(c.Code, storedNames, c.RegionId);

                        var name = LocalizationHelper.GetValue(storedNames, languageCode);
                        return new Country(c.Code, new Dictionary<string, string> {{languageCode, name}}, c.RegionId);
                    }).ToList();
            }, TimeSpan.FromDays(1));


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


        private async Task<Result<Location, ProblemDetails>> GetLocationFromGeoCoder(SearchLocation searchLocation)
        {
            var cacheKey = _flow.BuildKey(nameof(LocationService), GeoCoderPlace, searchLocation.PredictionResult.Source.ToString(), searchLocation.PredictionResult.Id);
            if (_flow.TryGetValue(cacheKey, out Location result))
                return Result.Ok<Location, ProblemDetails>(result);

            var (_, isFailure, place, error) = await _geoCoder.GetPlace(searchLocation.PredictionResult.Id, searchLocation.PredictionResult.SessionId);
            if (isFailure)
                return Result.Fail<Location, ProblemDetails>(ProblemDetailsBuilder.Build(error));

            if (place.Equals(default(Place)))
                return Result.Fail<Location, ProblemDetails>(ProblemDetailsBuilder.Build(
                    "A network error has been occurred. Please retry your request after several seconds.", HttpStatusCode.ServiceUnavailable));

            var viewPortDistance = CalculateDistance(place.Geometry.Viewport.NorthEast.Longitude, place.Geometry.Viewport.NorthEast.Latitude,
                place.Geometry.Viewport.SouthWest.Longitude, place.Geometry.Viewport.SouthWest.Latitude);
            var distance = (int) viewPortDistance / 2;

            var locality = place.Components.FirstOrDefault(c => c.Types.Contains("locality")).Name ?? string.Empty;
            var country = place.Components.FirstOrDefault(c => c.Types.Contains("country")).Name ?? string.Empty;

            result = new Location(place.Name, locality, country, place.Geometry.Location, distance, searchLocation.PredictionResult.Type);
            _flow.Set(cacheKey, result, TimeSpan.FromDays(1));

            return Result.Ok<Location, ProblemDetails>(result);
        }


        private const string CountriesKeyBase = "Countries";
        private const string GeoCoderPlace = "GeoCoderPlace";
        private const string PredictionsKeyBase = "Predictions";
        private const string RegionsKeyBase = "Regions";

        private const int DesirableNumberOfLocalPredictions = 5;
        private const int EarthRadiusInKm = 6371;

        private readonly EdoContext _context;
        private readonly IMemoryFlow _flow;
        private readonly IGeoCoder _geoCoder;
    }
}