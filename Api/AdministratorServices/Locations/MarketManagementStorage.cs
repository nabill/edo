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
    public class MarketManagementStorage : IMarketManagementStorage
    {
        public MarketManagementStorage(EdoContext context, IDoubleFlow flow)
        {
            _context = context;
            _flow = flow;
        }


        public Task<List<Market>> Get(CancellationToken cancellationToken)
            => _flow.GetOrSetAsync(_flow.BuildKey(nameof(MarketManagementService), MarketsKeyBase), async ()
                => await _context.Markets
                    .ToListAsync(cancellationToken), DefaultLocationCachingTime, cancellationToken)!;


        public Task<List<Country>> GetMarketCountries(int marketId, CancellationToken cancellationToken)
            => _flow.GetOrSetAsync(_flow.BuildKey(nameof(MarketManagementService), MarketsKeyBase, marketId.ToString()), async ()
                => await _context.Countries
                    .Where(c => c.MarketId == marketId)
                    .ToListAsync(cancellationToken), DefaultLocationCachingTime, cancellationToken)!;


        public async Task<Market?> Get(int marketId, CancellationToken cancellationToken)
        {
            var markets = await Get(cancellationToken);

            return markets.SingleOrDefault(m => m.Id == marketId);
        }


        public async Task Refresh(CancellationToken cancellationToken)
        {
            var markets = await LoadFromDatabase(cancellationToken);
            await _flow.SetAsync(_flow.BuildKey(nameof(MarketManagementService), MarketsKeyBase), markets, DefaultLocationCachingTime, cancellationToken);
        }


        private Task<List<Market>> LoadFromDatabase(CancellationToken cancellationToken)
            => _context.Markets.ToListAsync(cancellationToken);


        public async Task RefreshMarketCountries(int marketId, CancellationToken cancellationToken)
        {
            var countries = await LoadCountriesFromDatabase(marketId, cancellationToken);
            await _flow.SetAsync(_flow.BuildKey(nameof(MarketManagementService), MarketsKeyBase, marketId.ToString()), countries, DefaultLocationCachingTime, cancellationToken);
        }


        private Task<List<Country>> LoadCountriesFromDatabase(int marketId, CancellationToken cancellationToken)
            => _context.Countries.Where(c => c.MarketId == marketId).ToListAsync(cancellationToken);


        private static TimeSpan DefaultLocationCachingTime => TimeSpan.FromMinutes(10);

        private const string MarketsKeyBase = "Markets";

        private readonly IDoubleFlow _flow;
        private readonly EdoContext _context;
    }
}