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

namespace HappyTravel.Edo.Api.Services.Locations
{
    public class CountryService : ICountryService
    {
        public CountryService(EdoContext context, IDoubleFlow flow)
        {
            _context = context;
            _flow = flow;
        }


        public Task<List<Country>> Get(string query, string languageCode)
        {
            if (query == null || query.Length < 2)
                return GetFullCountryList(languageCode);

            return _flow.GetOrSetAsync(_flow.BuildKey(nameof(LocationService), CountriesKeyBase, languageCode, query), async ()
                => await _context.Countries
                    .Where(c => EF.Functions.ILike(c.Code, query) || EF.Functions.ILike((string)(object)c.Names, @$"%""{languageCode}"":%""%{query}%"))
                    .Select(c => new Country(c.Code, c.Names.RootElement.GetProperty(languageCode).GetString(), c.MarketId))
                    .ToListAsync(), DefaultLocationCachingTime);
        }


        public async ValueTask<string> GetCode(string countryName, string languageCode)
        {
            if (string.IsNullOrWhiteSpace(countryName))
                return string.Empty;

            var normalized = NormalizeCountryName(countryName);

            var cacheKey = _flow.BuildKey(nameof(CountryService), CodesKeyBase, languageCode, normalized);
            if (_flow.TryGetValue(cacheKey, out string result, DefaultLocationCachingTime))
                return result;

            var dictionary = await GetFullCountryDictionary(languageCode);
            if (!dictionary.TryGetValue(normalized, out result))
                return string.Empty;

            _flow.Set(cacheKey, result, DefaultLocationCachingTime);
            return result;
        }


        private static TimeSpan DefaultLocationCachingTime => TimeSpan.FromDays(1);


        private Task<Dictionary<string, string>> GetFullCountryDictionary(string languageCode)
        {
            var cacheKey = _flow.BuildKey(nameof(CountryService), CodesKeyBase, languageCode);
            return _flow.GetOrSetAsync(cacheKey, async () => (await GetFullCountryList(languageCode))
                    .ToDictionary(c => c.Name.ToUpperInvariant(), c => c.Code),
                DefaultLocationCachingTime);
        }


        private Task<List<Country>> GetFullCountryList(string languageCode)
        {
            var cacheKey = _flow.BuildKey(nameof(CountryService), CountriesKeyBase, languageCode);
            return _flow.GetOrSetAsync(cacheKey, async ()
                    => (await _context.Countries.ToListAsync())
                    .Select(c => new Country(c.Code, c.Names.RootElement.GetProperty(languageCode).GetString(), c.MarketId)).ToList(),
                DefaultLocationCachingTime);
        }


        private static string NormalizeCountryName(string countryName)
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
        private readonly IDoubleFlow _flow;
    }
}