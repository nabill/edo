using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Locations
{
    public class LocationService : ILocationService
    {
        public LocationService(EdoContext context, IDoubleFlow flow)
        {
            _context = context;
            _flow = flow;

            _countryService = new CountryService(context, flow);
        }


        public Task<List<Country>> GetCountries(string query, string languageCode)
            => _countryService.Get(query, languageCode);


        public Task<List<Region>> GetRegions(string languageCode)
            => _flow.GetOrSetAsync(_flow.BuildKey(nameof(LocationService), RegionsKeyBase, languageCode), async ()
                => (await _context.Regions.ToListAsync())
                .Select(r => new Region(r.Id, r.Names.RootElement.GetProperty(languageCode).ToString())).ToList(), DefaultLocationCachingTime)!;



        private static TimeSpan DefaultLocationCachingTime => TimeSpan.FromDays(1);

        private const string RegionsKeyBase = "Regions";

        private readonly EdoContext _context;
        private readonly CountryService _countryService;
        private readonly IDoubleFlow _flow;
    }
}