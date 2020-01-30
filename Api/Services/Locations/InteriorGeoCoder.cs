using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.Edo.Data;
using HappyTravel.Geography;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Locations
{
    public class InteriorGeoCoder : IGeoCoder
    {
        public InteriorGeoCoder(EdoContext context, ICountryService countryService)
        {
            _context = context;
            _countryService = countryService;
        }


        public async Task<Result<Location>> GetLocation(SearchLocation searchLocation, string languageCode)
        {
            var id = Guid.Parse(searchLocation.PredictionResult.Id);

            var location = await _context.Locations
                .Where(l => l.Id == id)
                .Select(l => new Location(l.Name, l.Locality, l.Country, new GeoPoint(l.Coordinates), l.DistanceInMeters, l.Source, l.Type, l.DataProviders))
                .FirstOrDefaultAsync();

            if (location.Equals(default))
                return Result.Fail<Location>($"No location with ID {searchLocation.PredictionResult.Id} has been found.");

            var name = location.Name.Length <= MinimalJsonFieldLength
                ? string.Empty
                : LocalizationHelper.GetValueFromSerializedString(location.Name, languageCode);

            var locality = location.Locality.Length <= MinimalJsonFieldLength
                ? string.Empty
                : LocalizationHelper.GetValueFromSerializedString(location.Locality, languageCode);

            var country = LocalizationHelper.GetValueFromSerializedString(location.Country, languageCode);
            var distance = searchLocation.DistanceInMeters != 0 ? searchLocation.DistanceInMeters : location.Distance;

            return Result.Ok(new Location(name, locality, country, location.Coordinates, distance, location.Source, location.Type, location.DataProviders));
        }


        public async ValueTask<Result<List<Prediction>>> GetLocationPredictions(string query, string sessionId, string languageCode)
        {
            var locations = await _context.SearchLocations(query, MaximumNumberOfPredictions).ToListAsync();

            var predictions = new List<Prediction>(locations.Count);
            foreach (var location in locations)
            {
                var predictionValue = BuildPredictionValue(location, languageCode);

                var countryName = LocalizationHelper.GetValueFromSerializedString(location.Country, LocalizationHelper.DefaultLanguageCode);
                var countryCode = await _countryService.GetCode(countryName, languageCode);

                predictions.Add(new Prediction(location.Id.ToString("N"), countryCode, location.Source, location.Type, predictionValue));
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


        private const int MaximumNumberOfPredictions = 10;
        private static readonly int MinimalJsonFieldLength = Infrastructure.Constants.Common.EmptyJsonFieldValue.Length;

        private readonly EdoContext _context;
        private readonly ICountryService _countryService;
    }
}