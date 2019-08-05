using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Locations
{
    public class CountryService : ICountryService
    {
        public CountryService(EdoContext context, IMemoryFlow flow)
        {
            _context = context;
            _flow = flow;
        }


        public ValueTask<List<Country>> Get(string query, string languageCode)
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


        public async ValueTask<string> GetCode(string countryName)
        {
            if (string.IsNullOrWhiteSpace(countryName))
                return string.Empty;

            var normalized = NormalizeCountryName(countryName);

            var cacheKey = _flow.BuildKey(nameof(CountryService), CodesKeyBase, normalized);
            if (_flow.TryGetValue(cacheKey, out string result))
                return result;

            var dictionary = await GetFullCountryDictionary();
            if (!dictionary.TryGetValue(normalized, out result))
                return string.Empty;

            _flow.Set(cacheKey, result, DefaultLocationCachingTime);
            return result;
        }


        private static TimeSpan DefaultLocationCachingTime => TimeSpan.FromDays(1);


        private ValueTask<Dictionary<string, string>> GetFullCountryDictionary()
            => _flow.GetOrSetAsync(_flow.BuildKey(nameof(CountryService), CodesKeyBase), async () =>
            {
                var countries = await GetFullCountryList(LocalizationHelper.DefaultLanguageCode);
                return countries.ToDictionary(c => LocalizationHelper.GetValue(c.Names, LocalizationHelper.DefaultLanguageCode).ToUpperInvariant(),
                    c => c.Code);
            }, DefaultLocationCachingTime);


        private ValueTask<List<Country>> GetFullCountryList(string languageCode)
            => _flow.GetOrSetAsync(_flow.BuildKey(nameof(CountryService), CountriesKeyBase, languageCode), async () =>
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


        private string NormalizeCountryName(string countryName)
        {
            var normalized = countryName.ToUpperInvariant();
            return CountryAliases.TryGetValue(normalized, out var result) ? result : normalized;
        }


        private const string CodesKeyBase = "CountryCodes";
        private const string CountriesKeyBase = "Countries";


        private static readonly Dictionary<string, string> CountryAliases = new Dictionary<string, string>
        {
            {"HONG KONG", "CHINA, HONG KONG SPECIAL ADMINISTRATIVE REGION"},
            {"LAOS", "LAO PEOPLE'S DEMOCRATIC REPUBLIC"},
            {"MACAO", "CHINA, MACAO SPECIAL ADMINISTRATIVE REGION"},
            {"NORTH KOREA", "DEMOCRATIC PEOPLE'S REPUBLIC OF KOREA"},
            {"SOUTH KOREA", "REPUBLIC OF KOREA"},
            {"UK", "UNITED KINGDOM OF GREAT BRITAIN AND NORTHERN IRELAND"},
            {"UNITED KINGDOM", "UNITED KINGDOM OF GREAT BRITAIN AND NORTHERN IRELAND"},
            {"USA", "UNITED STATES OF AMERICA"}
        };

        private readonly EdoContext _context;
        private readonly IMemoryFlow _flow;
    }
}