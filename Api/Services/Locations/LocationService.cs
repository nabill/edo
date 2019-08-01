using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using GeoAPI.Geometries;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Location = HappyTravel.Edo.Api.Models.Locations.Location;

namespace HappyTravel.Edo.Api.Services.Locations
{
    public class LocationService : ILocationService
    {
        public LocationService(EdoContext context, IMemoryFlow flow, IEnumerable<IGeoCoder> geoCoders, IGeometryFactory geometryFactory)
        {
            _context = context;
            _flow = flow;
            _geometryFactory = geometryFactory;

            _googleGeoCoder = geoCoders.First(c => c is GoogleGeoCoder);
            _interiorGeoCoder = geoCoders.First(c => c is InteriorGeoCoder);
        }


        public async ValueTask<Result<Location, ProblemDetails>> Get(SearchLocation searchLocation, string languageCode)
        {
            if (string.IsNullOrWhiteSpace(searchLocation.PredictionResult.Id))
                return Result.Ok<Location, ProblemDetails>(new Location(searchLocation.Coordinates, searchLocation.DistanceInMeters));

            if (searchLocation.PredictionResult.Type == LocationTypes.Unknown)
                return ProblemDetailsBuilder.BuildFailResult<Location>(
                    "Invalid prediction type. It looks like a prediction type was not specified in the request.");

            var cacheKey = _flow.BuildKey(nameof(LocationService), GeoCoderKey, searchLocation.PredictionResult.Source.ToString(),
                searchLocation.PredictionResult.Id);
            if (_flow.TryGetValue(cacheKey, out Location result))
                return Result.Ok<Location, ProblemDetails>(result);

            Result<Location> locationResult;
            switch (searchLocation.PredictionResult.Source)
            {
                case PredictionSources.Google:
                    locationResult = await _googleGeoCoder.GetLocation(searchLocation, languageCode);
                    break;
                case PredictionSources.NetstormingConnector:
                    locationResult = await _interiorGeoCoder.GetLocation(searchLocation, languageCode);
                    break;
                case PredictionSources.NotSpecified:
                default:
                    locationResult = Result.Fail<Location>($"'{nameof(searchLocation.PredictionResult.Source)}' is empty or wasn't specified in your request.");
                    break;
            }

            if (locationResult.IsFailure)
                return ProblemDetailsBuilder.BuildFailResult<Location>(locationResult.Error, HttpStatusCode.ServiceUnavailable);

            result = locationResult.Value;
            _flow.Set(cacheKey, result, DefaultLocationCachingTime);

            return Result.Ok<Location, ProblemDetails>(result);
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
            }, DefaultLocationCachingTime);
        }


        public async ValueTask<Result<List<Prediction>, ProblemDetails>> GetPredictions(string query, string sessionId, string languageCode)
        {
            query = query.ToLowerInvariant();

            var cacheKey = _flow.BuildKey(nameof(LocationService), PredictionsKeyBase, languageCode, query);
            if (_flow.TryGetValue(cacheKey, out List<Prediction> predictions))
                return Result.Ok<List<Prediction>, ProblemDetails>(predictions);

            (_, _, predictions, _) = await _interiorGeoCoder.GetLocationPredictions(query, sessionId, languageCode);

            if (predictions.Count >= DesirableNumberOfLocalPredictions)
            {
                _flow.Set(cacheKey, SortPredictions(predictions), DefaultLocationCachingTime);
                return Result.Ok<List<Prediction>, ProblemDetails>(SortPredictions(predictions));
            }

            var (_, isFailure, googlePredictions, error) = await _googleGeoCoder.GetLocationPredictions(query, sessionId, languageCode);
            if (isFailure && !predictions.Any())
                return ProblemDetailsBuilder.BuildFailResult<List<Prediction>>(error);

            if (googlePredictions != null)
                predictions.AddRange(googlePredictions);

            var sorted = SortPredictions(predictions);
            _flow.Set(cacheKey, sorted, DefaultLocationCachingTime);

            return Result.Ok<List<Prediction>, ProblemDetails>(sorted);
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
            }, DefaultLocationCachingTime);


        public async Task Set(IEnumerable<Location> locations)
        {
            var locationList = locations.ToList();
            var added = new List<Data.Locations.Location>(locationList.Count);
            foreach (var location in locationList)
                added.Add(new Data.Locations.Location
                {
                    Locality = location.Locality.AsSpan().IsEmpty
                        ? Infrastructure.Constants.Common.EmptyJsonFieldValue
                        : location.Locality,
                    Coordinates = _geometryFactory.CreatePoint(new Coordinate(location.Coordinates.Longitude, location.Coordinates.Latitude)),
                    Country = location.Country,
                    DistanceInMeters = location.Distance,
                    Name = location.Name.AsSpan().IsEmpty
                        ? Infrastructure.Constants.Common.EmptyJsonFieldValue
                        : location.Name,
                    Source = location.Source,
                    Type = location.Type
                });

            _context.AddRange(added);
            await _context.SaveChangesAsync();
        }


        private static TimeSpan DefaultLocationCachingTime => TimeSpan.FromDays(1);


        private ValueTask<List<Country>> GetFullCountryList(string languageCode)
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
            }, DefaultLocationCachingTime);


        private static List<Prediction> SortPredictions(List<Prediction> target)
            => target.OrderBy(p => Array.IndexOf(PredictionTypeSortOrder, p.Type))
                .ThenBy(p => Array.IndexOf(PredictionSourceSortOrder, p.Source))
                .ToList();


        private const string CountriesKeyBase = "Countries";
        private const string GeoCoderKey = "GeoCoder";
        private const string PredictionsKeyBase = "Predictions";
        private const string RegionsKeyBase = "Regions";

        private const int DesirableNumberOfLocalPredictions = 5;

        private static readonly PredictionSources[] PredictionSourceSortOrder =
        {
            PredictionSources.NetstormingConnector,
            PredictionSources.Google,
            PredictionSources.NotSpecified
        };

        private static readonly LocationTypes[] PredictionTypeSortOrder =
        {
            LocationTypes.Hotel,
            LocationTypes.Location,
            LocationTypes.Destination,
            LocationTypes.Landmark,
            LocationTypes.Unknown
        };

        private readonly EdoContext _context;
        private readonly IMemoryFlow _flow;
        private readonly IGeometryFactory _geometryFactory;
        private readonly IGeoCoder _googleGeoCoder;
        private readonly IGeoCoder _interiorGeoCoder;
    }
}