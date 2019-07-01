using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.Edo.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Locations
{
    public class LocationService : ILocationService
    {
        public LocationService(EdoContext context, IMemoryFlow flow, IGeocoder geocoder)
        {
            _context = context;
            _flow = flow;
            _geocoder = geocoder;
        }


        public ValueTask<List<Country>> GetCountries(string query, string languageCode)
        {
            if (query?.Length < 2)
                return GetFullCountryList(languageCode);

            return _flow.GetOrSetAsync(_flow.BuildKey(CountriesKeyBase, languageCode, query), async () =>
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
            => _flow.GetOrSetAsync(_flow.BuildKey(PredictionsKeyBase, languageCode, query), async () =>
            {
                var localResults = new List<Prediction>();
                if (localResults.Count < DesirableNumberOfLocalPredictions)
                {
                    var (_, isFailure, predictions, error) = await _geocoder.GetLocationPredictions(query, session, languageCode);
                    if (isFailure)
                        return Result.Fail<List<Prediction>, ProblemDetails>(new ProblemDetails
                        {
                            Detail = error,
                            Status = (int) HttpStatusCode.BadRequest
                        });

                    localResults.AddRange(predictions);
                }

                return Result.Ok<List<Prediction>, ProblemDetails>(localResults);
            }, TimeSpan.FromDays(1));


        public ValueTask<List<Region>> GetRegions(string languageCode)
            => _flow.GetOrSetAsync(_flow.BuildKey(RegionsKeyBase, languageCode), async () =>
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
            => _flow.GetOrSetAsync(_flow.BuildKey(CountriesKeyBase, languageCode), async () =>
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


        private const string CountriesKeyBase = "Countries";
        private const string PredictionsKeyBase = "Predictions";
        private const string RegionsKeyBase = "Regions";

        private const int DesirableNumberOfLocalPredictions = 5;

        private readonly EdoContext _context;
        private readonly IMemoryFlow _flow;
        private readonly IGeocoder _geocoder;
    }
}