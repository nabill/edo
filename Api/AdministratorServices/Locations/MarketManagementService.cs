using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Locations;
using Microsoft.EntityFrameworkCore;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using Api.Models.Locations;

namespace Api.AdministratorServices.Locations
{
    public class MarketManagementService : IMarketManagementService
    {
        public MarketManagementService(EdoContext context, IMarketManagementStorage marketStorage)
        {
            _context = context;
            _marketStorage = marketStorage;
        }


        public Task<Result> Add(string languageCode, MarketRequest marketRequest, CancellationToken cancellationToken = default)
        {
            return Validate(languageCode, marketRequest)
                .Tap(Add)
                .Tap(() => _marketStorage.Refresh(cancellationToken));

            async Task Add()
            {
                var newMarket = new Market()
                {
                    Names = marketRequest.Names!
                };

                _context.Add(newMarket);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }


        public Task<List<Market>> Get(CancellationToken cancellationToken = default)
            => _marketStorage.Get(cancellationToken);


        public Task<Result> Update(string languageCode, MarketRequest marketRequest, CancellationToken cancellationToken = default)
        {
            return Validate(languageCode, marketRequest)
                .BindWithTransaction(_context, () => GetMarketById(marketRequest.MarketId!.Value, cancellationToken)
                    .Bind(Update))
                .Tap(() => _marketStorage.Refresh(cancellationToken));

            async Task<Result> Update(Market marketData)
            {
                marketData.Names = marketRequest.Names!;

                _context.Update(marketData);
                await _context.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
        }


        public Task<Result> Remove(int marketId, CancellationToken cancellationToken = default)
        {
            return Result.Success()
                .BindWithTransaction(_context, () => GetMarketById(marketId, cancellationToken)
                    .Bind(Remove))
                .Tap(() => _marketStorage.Refresh(cancellationToken));

            async Task<Result> Remove(Market marketData)
            {
                _context.Remove(marketData);
                await _context.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
        }


        private async Task<Result<Market>> GetMarketById(int marketId, CancellationToken cancellationToken)
        {
            var market = await _marketStorage.Get(marketId, cancellationToken);

            if (market is not null)
                return Result.Success(market);

            market = await _context.Markets
                .SingleOrDefaultAsync(m => m.Id == marketId, cancellationToken);

            if (market == default)
                return Result.Failure<Market>($"Market with Id {marketId} was not found");

            return Result.Success(market);
        }


        private Result Validate(string languageCode, MarketRequest marketRequest)
        {
            if (marketRequest.Names is null)
                return Result.Failure("Request doesn't contain any names by language code.");

            var value = string.Empty;
            var hasCurrentLanguageCode = marketRequest.Names.TryGetValue(languageCode, out value);
            var hasDefaultLanguageCode = marketRequest.Names.TryGetValue(LocalizationHelper.DefaultLanguageCode, out value);

            if (!hasCurrentLanguageCode && !hasDefaultLanguageCode)
                return Result.Failure("Request need to be contained at least current language code or default language code.");

            return Result.Success();
        }


        private readonly EdoContext _context;
        private readonly IMarketManagementStorage _marketStorage;
    }
}