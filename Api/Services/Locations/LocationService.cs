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

            _countryService = new CountryService(context, flow);
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


        public ValueTask<List<Country>> GetCountries(string query, string languageCode) => _countryService.Get(query, languageCode);


        public async ValueTask<Result<List<Prediction>, ProblemDetails>> GetPredictions(string query, string sessionId, string languageCode)
        {
            if (query.Length < 3)
                return Result.Ok<List<Prediction>, ProblemDetails>(new List<Prediction>(0));

            query = query.ToLowerInvariant();

            var cacheKey = _flow.BuildKey(nameof(LocationService), PredictionsKeyBase, languageCode, query);
            if (_flow.TryGetValue(cacheKey, out List<Prediction> predictions))
                return Result.Ok<List<Prediction>, ProblemDetails>(predictions);

            (_, _, predictions, _) = await _interiorGeoCoder.GetLocationPredictions(query, sessionId, languageCode);

            if (DesirableNumberOfLocalPredictions < predictions.Count)
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


        private static List<Prediction> SortPredictions(List<Prediction> target)
            => target.OrderBy(p => Array.IndexOf(PredictionTypeSortOrder, p.Type))
                .ThenBy(p => Array.IndexOf(PredictionSourceSortOrder, p.Source))
                .ToList();


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
        private readonly CountryService _countryService;
        private readonly IMemoryFlow _flow;
        private readonly IGeometryFactory _geometryFactory;
        private readonly IGeoCoder _googleGeoCoder;
        private readonly IGeoCoder _interiorGeoCoder;
    }
}