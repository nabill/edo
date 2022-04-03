using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.Edo.Data;
using MarketContext = HappyTravel.Edo.Data.Locations.Market;
using Microsoft.EntityFrameworkCore;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;

namespace Api.AdministratorServices.Locations
{
    public class MarketManagementService : IMarketManagementService
    {
        public MarketManagementService(EdoContext context, IDoubleFlow flow)
        {
            _context = context;
            _flow = flow;
        }


        public Task<Result> AddMarket(string languageCode, JsonDocument namesRequest, CancellationToken cancellationToken = default)
        {
            return Validate(languageCode, namesRequest)
                .Tap(Add);

            async Task Add()
            {
                var newMarket = new MarketContext()
                {
                    Names = namesRequest
                };

                _context.Add(newMarket);
                await _context.SaveChangesAsync();
            }
        }


        public Task<List<Market>> GetMarkets(string languageCode, CancellationToken cancellationToken = default)
            => _flow.GetOrSetAsync(_flow.BuildKey(nameof(MarketManagementService), MarketsKeyBase, languageCode), async ()
               => await _context.Markets
                    .Select(r => new Market(r.Id, r.Names.RootElement.GetProperty(languageCode).GetString()!))
                    .ToListAsync(cancellationToken), DefaultLocationCachingTime)!;


        public Task<Result> ModifyMarket(string languageCode, int marketId, JsonDocument namesRequest, CancellationToken cancellationToken = default)
        {
            return Validate(languageCode, namesRequest)
                .BindWithTransaction(_context, () => GetMarketById(marketId)
                    .Bind(Update));

            async Task<Result> Update(MarketContext marketContext)
            {
                marketContext.Names = namesRequest;

                _context.Update(marketContext);
                await _context.SaveChangesAsync();

                return Result.Success();
            }
        }


        public Task<Result> RemoveMarket(int marketId, CancellationToken cancellationToken = default)
        {
            return Result.Success()
                .BindWithTransaction(_context, () => GetMarketById(marketId)
                    .Bind(Remove));

            async Task<Result> Remove(MarketContext marketContext)
            {
                _context.Remove(marketContext);
                await _context.SaveChangesAsync();

                return Result.Success();
            }
        }


        private async Task<Result<MarketContext>> GetMarketById(int marketId)
        {
            var market = await _context.Markets
                .SingleOrDefaultAsync(m => m.Id == marketId);

            if (market == default)
                return Result.Failure<MarketContext>($"Market with Id {marketId} not found");

            return Result.Success(market);
        }


        private Result Validate(string languageCode, JsonDocument namesRequest)
        {
            var value = new JsonElement();
            var hasCurrentLanguageCode = namesRequest.RootElement.TryGetProperty(languageCode, out value);
            var hasDefaultLanguageCode = namesRequest.RootElement.TryGetProperty(LocalizationHelper.DefaultLanguageCode, out value);

            if (!hasCurrentLanguageCode && !hasDefaultLanguageCode)
                return Result.Failure("Request need to be contained at least current language code or default language code");

            return Result.Success();
        }


        private static TimeSpan DefaultLocationCachingTime => TimeSpan.FromDays(1);

        private const string MarketsKeyBase = "Markets";

        private readonly EdoContext _context;
        private readonly IDoubleFlow _flow;
    }
}