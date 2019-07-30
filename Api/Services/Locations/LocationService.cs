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


        public async ValueTask<Result<Location, ProblemDetails>> Get(SearchLocation searchLocation)
        {
            if (string.IsNullOrWhiteSpace(searchLocation.PredictionResult.Id))
                return Result.Ok<Location, ProblemDetails>(new Location(string.Empty, string.Empty, string.Empty, searchLocation.Coordinates,
                    searchLocation.DistanceInMeters, PredictionSources.NotSpecified, LocationTypes.Unknown));

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


        public async ValueTask<Result<List<Prediction>, ProblemDetails>> GetPredictions(string query, string sessionId, string languageCode)
        {
            query = query.ToLowerInvariant();

            var cacheKey = _flow.BuildKey(nameof(LocationService), PredictionsKeyBase, languageCode, query);
            if (_flow.TryGetValue(cacheKey, out List<Prediction> predictions))
                return Result.Ok<List<Prediction>, ProblemDetails>(predictions);

            (_, _, predictions, _) = await _interiorGeoCoder.GetLocationPredictions(query, sessionId, languageCode);

            if (predictions.Count >= DesirableNumberOfLocalPredictions)
            {
                _flow.Set(cacheKey, SortPredictions(predictions), TimeSpan.FromDays(1));
                return Result.Ok<List<Prediction>, ProblemDetails>(SortPredictions(predictions));
            }

            var (_, isFailure, googlePredictions, error) = await _googleGeoCoder.GetLocationPredictions(query, sessionId, languageCode);
            if (isFailure && !predictions.Any())
                return Result.Fail<List<Prediction>, ProblemDetails>(new ProblemDetails
                {
                    Detail = error,
                    Status = (int) HttpStatusCode.BadRequest
                });

            if (googlePredictions != null)
                predictions.AddRange(googlePredictions);

            var sorted = SortPredictions(predictions);
            _flow.Set(cacheKey, sorted, TimeSpan.FromDays(1));

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
            }, TimeSpan.FromDays(1));


        public async Task Set(PredictionSources source, IEnumerable<Location> locations)
        {
            var enumerable = locations.ToList();
            var added = new List<Data.Locations.Location>(enumerable.Count);
            foreach (var location in enumerable)
            {
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
                    Source = source,
                    Type = location.Type
                });
            }

            _context.AddRange(added);
            await _context.SaveChangesAsync();
        }


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
            }, TimeSpan.FromDays(1));


        private async Task<Result<Location, ProblemDetails>> GetLocationFromGeoCoder(SearchLocation searchLocation)
        {
            var cacheKey = _flow.BuildKey(nameof(LocationService), GeoCoderPlace, searchLocation.PredictionResult.Source.ToString(), searchLocation.PredictionResult.Id);
            if (_flow.TryGetValue(cacheKey, out Location result))
                return Result.Ok<Location, ProblemDetails>(result);

            var (_, isFailure, location, error) = await _googleGeoCoder.GetLocation(searchLocation.PredictionResult.Id, searchLocation.PredictionResult.SessionId, searchLocation.PredictionResult.Type);
            if (isFailure)
                return Result.Fail<Location, ProblemDetails>(ProblemDetailsBuilder.Build(error, HttpStatusCode.ServiceUnavailable));

            _flow.Set(cacheKey, location, TimeSpan.FromDays(1));
            return Result.Ok<Location, ProblemDetails>(result);
        }


        private List<Prediction> SortPredictions(List<Prediction> target) 
            => target.OrderBy(p => Array.IndexOf(PredictionSortOrder, p.Type))
                .ToList();


        private const string CountriesKeyBase = "Countries";
        private const string GeoCoderPlace = "GeoCoderPlace";
        private const string PredictionsKeyBase = "Predictions";
        private const string RegionsKeyBase = "Regions";

        private const int DesirableNumberOfLocalPredictions = 5;
        private static readonly LocationTypes[] PredictionSortOrder = {
            LocationTypes.Hotel,
            LocationTypes.Location,
            LocationTypes.Destination,
            LocationTypes.Landmark,
            LocationTypes.Unknown
        };

        private readonly EdoContext _context;
        private readonly IMemoryFlow _flow;
        private readonly IGeoCoder _googleGeoCoder;
        private readonly IGeoCoder _interiorGeoCoder;
        private readonly IGeometryFactory _geometryFactory;
    }
}