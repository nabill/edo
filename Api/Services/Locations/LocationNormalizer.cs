using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Locations;
using LocationNameNormalizer;
using LocationNameNormalizer.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Locations
{
    public class LocationNormalizer : ILocationNormalizer
    {
        public LocationNormalizer(EdoContext edoContext, ILocationNameNormalizer locationNameNormalizer, ILogger<LocationNormalizer> logger)
        {
            _logger = logger;
            _edoContext = edoContext;
            _locationNameNormalizer = locationNameNormalizer;
        }


        public async Task StartNormalization()
        {
            await foreach (var locations in GetLocations(BatchSize))
            {
                Normalize(locations);
                _edoContext.Locations.UpdateRange(locations);
                await _edoContext.SaveChangesAsync();
                _logger.LogLocationNormalized($"Locations have been normalized: {locations.Count}");
            }
        }


        private void Normalize(Location location)
        {
            NormalizeCountry(location);
            NormalizeLocality(location);
            NormalizeName(location);
        }


        private void Normalize(List<Location> locations)
        {
            foreach (var location in locations)
            {
                Normalize(location);
            }
        }


        private void NormalizeCountry(Location location)
        {
            if (location.Country == "{}")
                return;

            var values = LocalizationHelper.GetValues(location.Country);
            if (!values.ContainsKey(LocalizationHelper.DefaultLanguageCode))
            {
                LogDefaultLanguageKeyIsMissingInFieldOfLocationsTable(nameof(location.Locality), location.Country, location);
                return;
            }

            var normalizedCountry = _locationNameNormalizer.GetNormalizedCountryName(values[LocalizationHelper.DefaultLanguageCode]);
            values[LocalizationHelper.DefaultLanguageCode] = normalizedCountry;
            location.DefaultCountry = normalizedCountry;
            location.Country = JsonConvert.SerializeObject(values, _jsonSettings);
        }


        private void NormalizeLocality(Location location)
        {
            if (location.Locality == "{}")
                return;
            
            var values = LocalizationHelper.GetValues(location.Locality);
            if (!values.ContainsKey(LocalizationHelper.DefaultLanguageCode))
            {
                LogDefaultLanguageKeyIsMissingInFieldOfLocationsTable(nameof(location.Locality), location.Locality, location);
                return;
            }
            
            var normalizedLocality = _locationNameNormalizer.GetNormalizedLocalityName(location.DefaultCountry, values[LocalizationHelper.DefaultLanguageCode]);
            values[LocalizationHelper.DefaultLanguageCode] = normalizedLocality;
            location.DefaultLocality = normalizedLocality;
            location.Locality = JsonConvert.SerializeObject(values, _jsonSettings);
        }


        private void NormalizeName(Location location)
        {
            if (location.Name == "{}")
                return;

            var values = LocalizationHelper.GetValues(location.Name);
            if (!values.ContainsKey(LocalizationHelper.DefaultLanguageCode))
            {
                LogDefaultLanguageKeyIsMissingInFieldOfLocationsTable(nameof(location.Name), location.Name, location);
                return;
            }
            
            var normalizedDefaultValue = values[LocalizationHelper.DefaultLanguageCode].ToNormalizedName();
            values[LocalizationHelper.DefaultLanguageCode] = normalizedDefaultValue;
            location.DefaultName = normalizedDefaultValue;
            location.Name = JsonConvert.SerializeObject(values, _jsonSettings);
        }


        public async IAsyncEnumerable<List<Location>> GetLocations(int batchSize)
        {
            var skip = 0;
            List<Location> locations;
            do
            {
                locations = await _edoContext.Locations
                    .OrderBy(i => i.Id)
                    .Skip(skip)
                    .Take(batchSize)
                    .ToListAsync();
                
                yield return locations;
                
                skip += batchSize;
            } while (locations.Count == batchSize);
        }


        private void LogDefaultLanguageKeyIsMissingInFieldOfLocationsTable(string fieldName, string fieldValue, Location location) =>
            _logger.LogDefaultLanguageKeyIsMissingInFieldOfLocationsTable($"Failed to get {nameof(LocalizationHelper.DefaultLanguageCode)} from {fieldName}: {fieldValue}, {nameof(_edoContext.Locations)}, {nameof(location.Id)}: {location.Id}");
        

        
        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore};

        private const int BatchSize = 100000;
        private readonly EdoContext _edoContext;
        private readonly ILocationNameNormalizer _locationNameNormalizer;
        private readonly ILogger<LocationNormalizer> _logger;
    }
}