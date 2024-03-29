using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Locations;
using Microsoft.EntityFrameworkCore;

namespace Api.AdministratorServices.Locations
{
    public class CountryManagementStorage : ICountryManagementStorage
    {
        public CountryManagementStorage(EdoContext context, IDoubleFlow flow)
        {
            _context = context;
            _flow = flow;
        }


        public Task<List<Country>> Get(CancellationToken cancellationToken)
            => _flow.GetOrSetAsync(_flow.BuildKey(nameof(CountryManagementStorage), CountryKeyBase), async ()
                => await _context.Countries.ToListAsync(cancellationToken), DefaultLocationCachingTime, cancellationToken)!;


        public Task Refresh(CancellationToken cancellationToken)
            => _flow.SetAsync(_flow.BuildKey(nameof(CountryManagementStorage), CountryKeyBase), async ()
                => await _context.Countries.ToListAsync(cancellationToken), DefaultLocationCachingTime, cancellationToken)!;


        public async Task AddRange(List<Country> countries, CancellationToken cancellationToken)
        {
            _context.Countries.AddRange(countries);
            await _context.SaveChangesAsync(cancellationToken);
        }


        private static TimeSpan DefaultLocationCachingTime => TimeSpan.FromMinutes(10);

        private const string CountryKeyBase = "Countries";

        private readonly IDoubleFlow _flow;
        private readonly EdoContext _context;
    }
}