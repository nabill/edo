using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Models.Locations.Google;
using HappyTravel.Edo.Data;
using HappyTravel.Geography;
using Microsoft.EntityFrameworkCore;
using Prediction = HappyTravel.Edo.Api.Models.Locations.Prediction;

namespace HappyTravel.Edo.Api.Services.Locations
{
    public class InteriorGeoCoder : IGeoCoder
    {
        public InteriorGeoCoder(EdoContext context, ICountryService countryService)
        {
            _context = context;
            _countryService = countryService;
        }


        public async Task<Result<EdoContracts.GeoData.Location>> GetLocation(SearchLocation searchLocation, string languageCode)
        {
            var id = Guid.Parse(searchLocation.PredictionResult.Id);

            var location = await _context.Locations
                .Where(l => l.Id == id)
                .Select(l => new EdoContracts.GeoData.Location(l.Name, l.Locality, l.Country, new GeoPoint(l.Coordinates), l.DistanceInMeters, l.Source, l.Type))
                .FirstOrDefaultAsync();

            if (location.Equals(default))
                return Result.Fail<EdoContracts.GeoData.Location>($"No location with ID {searchLocation.PredictionResult.Id} has been found.");

            var name = location.Name.Length <= MinimalJsonFieldLength
                ? string.Empty
                : LocalizationHelper.GetValueFromSerializedString(location.Name, languageCode);

            var locality = location.Locality.Length <= MinimalJsonFieldLength
                ? string.Empty
                : LocalizationHelper.GetValueFromSerializedString(location.Locality, languageCode);

            var country = LocalizationHelper.GetValueFromSerializedString(location.Country, languageCode);
            var distance = searchLocation.DistanceInMeters != 0 ? searchLocation.DistanceInMeters : location.Distance;

            return Result.Ok(new EdoContracts.GeoData.Location(name, locality, country, location.Coordinates, distance, location.Source, location.Type));
        }


        public async ValueTask<Result<List<Prediction>>> GetLocationPredictions(string query, string sessionId, string languageCode)
        {
            var locations = await _context.SearchLocations(query, MaximumNumberOfPredictions).ToListAsync();

            var predictions = new List<Prediction>(locations.Count);
            foreach (var location in locations)
            {
                var predictionValue = BuildPredictionValue(location, languageCode);
                var matches = GetMatches(predictionValue, query);

                var countryName = LocalizationHelper.GetValueFromSerializedString(location.Country, LocalizationHelper.DefaultLanguageCode);
                var countryCode = await _countryService.GetCode(countryName);

                predictions.Add(new Prediction(location.Id.ToString("N"), countryCode, location.Source, matches, location.Type, predictionValue));
            }

            return Result.Ok(predictions);
        }


        private static string BuildPredictionValue(Data.Locations.Location location, string languageCode)
        {
            var result = string.Empty;
            if (MinimalJsonFieldLength < location.Name.Length)
            {
                var name = LocalizationHelper.GetValueFromSerializedString(location.Name, languageCode);
                if (name != string.Empty)
                    result = name;
            }

            if (MinimalJsonFieldLength < location.Locality.Length)
            {
                var locality = LocalizationHelper.GetValueFromSerializedString(location.Locality, languageCode);
                if (locality != string.Empty)
                {
                    if (result != string.Empty)
                        result += ", " + locality;
                    else
                        result = locality;
                }
            }

            var country = LocalizationHelper.GetValueFromSerializedString(location.Country, languageCode);
            if (result != string.Empty)
                result += ", " + country;
            else
                result = country;

            return result;
        }


        private static List<Match> GetMatches(in ReadOnlySpan<char> predictionValue, in ReadOnlySpan<char> query)
        {
            var results = new List<Match>();
            var length = query.Length;
            var temp = predictionValue;
            var totalOffset = 0;
            while (true)
            {
                var offset = temp.IndexOf(query, StringComparison.InvariantCultureIgnoreCase);
                if (offset == -1)
                    return results;

                results.Add(new Match(length, offset + totalOffset));

                if (temp.Length < offset + 1)
                    return results;

                temp = temp.Slice(offset + 1);
                totalOffset += offset;
            }
        }


        private const int MaximumNumberOfPredictions = 10;
        private static readonly int MinimalJsonFieldLength = Infrastructure.Constants.Common.EmptyJsonFieldValue.Length;

        private readonly EdoContext _context;
        private readonly ICountryService _countryService;
    }
}