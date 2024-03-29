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
using ApiModels = HappyTravel.Edo.Api.Models.Locations;
using FluentValidation;
using System;
using System.Linq;

namespace Api.AdministratorServices.Locations
{
    public class MarketManagementService : IMarketManagementService
    {
        public MarketManagementService(EdoContext context, IMarketManagementStorage marketStorage,
            ICountryManagementStorage countryStorage)
        {
            _context = context;
            _marketStorage = marketStorage;
            _countryStorage = countryStorage;
        }


        public Task<Result> Add(string languageCode, MarketRequest marketRequest, CancellationToken cancellationToken = default)
        {
            return Result.Success()
                .Tap(Add)
                .Tap(() => _marketStorage.Refresh(cancellationToken));


            async Task Add()
            {
                var newMarket = new Market()
                {
                    Names = new HappyTravel.MultiLanguage.MultiLanguage<string>
                    {
                        // Hard-coded until we will be back to multilanguage model
                        En = marketRequest.Name
                    }
                };

                _context.Add(newMarket);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }


        public async Task<List<ApiModels.Market>> Get(CancellationToken cancellationToken = default)
        {
            var markets = await _marketStorage.Get(cancellationToken);
            return markets
                .Select(ToApiProjection())
                .OrderBy(m => m.Name)
                .ToList();

            static Func<Market, ApiModels.Market> ToApiProjection()
                => market => new ApiModels.Market(market.Id, market.Names.GetValueOrDefault(LocalizationHelper.DefaultLanguageCode));
        }


        public Task<Result> Update(string languageCode, MarketRequest marketRequest, CancellationToken cancellationToken = default)
        {
            return ValidateUpdate()
                .BindWithTransaction(_context, () => GetMarketById(marketRequest.MarketId!.Value, cancellationToken)
                    .Bind(Update))
                .Tap(() => _marketStorage.Refresh(cancellationToken));


            Result ValidateUpdate()
                => GenericValidator<int>.Validate(v =>
                    {
                        v.RuleFor(m => m)
                            .GreaterThan(0)
                            .NotEqual(UnknownMarketId)
                            .WithMessage("Updating unknown market is forbidden");
                    }, marketRequest.MarketId!.Value);


            async Task<Result> Update(Market marketData)
            {
                // Hard-coded until we will be back to multilanguage model
                marketData.Names.En = marketRequest.Name;

                _context.Update(marketData);
                await _context.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
        }


        public Task<Result> Remove(int marketId, CancellationToken cancellationToken = default)
        {
            return ValidateRemove()
                .BindWithTransaction(_context, () => GetMarketById(marketId, cancellationToken)
                    .Bind(Remove)
                    .Tap(ReleaseCountries))
                .Tap(CacheRefresh);


            Result ValidateRemove()
                => GenericValidator<int>.Validate(v =>
                    {
                        v.RuleFor(m => m)
                            .GreaterThan(0)
                            .NotEqual(UnknownMarketId)
                            .WithMessage("Removing unknown market is forbidden");
                    }, marketId);


            async Task<Result> Remove(Market marketData)
            {
                _context.Remove(marketData);
                await _context.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }


            async Task ReleaseCountries()
            {
                var countries = await _context.Countries
                    .Where(c => c.MarketId == marketId)
                    .ToListAsync(cancellationToken);

                countries.ForEach(c => c.MarketId = UnknownMarketId);

                _context.UpdateRange(countries);
                await _context.SaveChangesAsync(cancellationToken);

                await _countryStorage.Refresh(cancellationToken);
                countries.ForEach(async c => await _marketStorage.RefreshCountry(c, cancellationToken));
            }


            async Task CacheRefresh()
            {
                await _marketStorage.Refresh(cancellationToken);
                await _marketStorage.RefreshMarketCountries(UnknownMarketId, cancellationToken);
                await _marketStorage.RefreshMarketCountries(marketId, cancellationToken);
            }
        }


        public Task<Result> UpdateMarketCountries(CountryRequest request, CancellationToken cancellationToken)
        {
            return ValidateUpdate()
                .Tap(SetDifferencesToUnkownMarket)
                .Tap(Update)
                .Tap(() => _countryStorage.Refresh(cancellationToken))
                .Tap(() => _marketStorage.RefreshMarketCountries(request.MarketId, cancellationToken))
                .Tap(RefreshUnknownMarketCountries);


            Task<Result> ValidateUpdate()
                => GenericValidator<CountryRequest>.ValidateAsync(v =>
                    {
                        v.RuleFor(r => r.MarketId)
                            .NotNull()
                            .MustAsync(IsExist())
                            .WithMessage($"Market with Id {request.MarketId} was not found")
                            .NotEqual(UnknownMarketId)
                            .WithMessage("Updating unknown market's countries is forbidden");

                        v.RuleFor(r => r.CountryCodes)
                            .NotNull()
                            .ForEach(c => c
                                .MustAsync(CheckCountryAvailability(request.MarketId, UnknownMarketId))
                                .WithMessage("One or many of the country's codes are not available"));


                        Func<string, CancellationToken, Task<bool>> CheckCountryAvailability(int currentMarketId, int unknownMarketId)
                            => async (countryCode, cancellationToken)
                                => await _context.Countries
                                    .AnyAsync(c => c.Code == countryCode && (c.MarketId == currentMarketId || c.MarketId == unknownMarketId), cancellationToken);
                    }, request);


            async Task SetDifferencesToUnkownMarket()
            {
                var countriesDifferences = await _context.Countries
                    .Where(m => m.MarketId == request.MarketId && !request.CountryCodes!.Contains(m.Code))
                    .ToListAsync(cancellationToken);

                countriesDifferences.ForEach(c =>
                {
                    c.MarketId = UnknownMarketId;
                });

                _context.UpdateRange(countriesDifferences);
                await _context.SaveChangesAsync(cancellationToken);
            }


            async Task Update()
            {
                var countries = await _context.Countries
                    .Where(m => request.CountryCodes!.Contains(m.Code))
                    .ToListAsync(cancellationToken);

                countries.ForEach(c =>
                {
                    c.MarketId = request.MarketId;
                });

                _context.UpdateRange(countries);
                await _context.SaveChangesAsync(cancellationToken);
            }


            async Task RefreshUnknownMarketCountries()
                => await _marketStorage.RefreshMarketCountries(UnknownMarketId, cancellationToken);
        }


        public Task<Result<List<ApiModels.CountrySlim>>> GetMarketCountries(int marketId, CancellationToken cancellationToken)
        {
            return ValidateGet()
                .Bind(Get)
                .Map(ToApiModel);


            Task<Result> ValidateGet()
                => GenericValidator<int>.ValidateAsync(v =>
                    {
                        v.RuleFor(r => r)
                                .NotNull()
                                .MustAsync(IsExist())
                                .WithMessage($"Market with Id {marketId} was not found");
                    }, marketId);


            async Task<Result<List<Country>>> Get()
                => await _marketStorage.GetMarketCountries(marketId, cancellationToken);


            List<ApiModels.CountrySlim> ToApiModel(List<Country> countries)
                => countries
                    .Select(c => new ApiModels.CountrySlim(c.Code, c.Names.En))
                    .ToList();
        }


        private async Task<Result<Market>> GetMarketById(int marketId, CancellationToken cancellationToken)
        {
            var market = await _context.Markets
                .SingleOrDefaultAsync(m => m.Id == marketId, cancellationToken);

            if (market == default)
                return Result.Failure<Market>($"Market with Id {marketId} was not found");

            return Result.Success(market);
        }


        private Func<int, CancellationToken, Task<bool>> IsExist()
            => async (marketId, cancellationToken)
                => await _context.Markets.AnyAsync(m => m.Id == marketId, cancellationToken);


        // Commented until we will be back to multilanguage model
        // private Result Validate(string languageCode, MarketRequest marketRequest)
        // {
        //     if (marketRequest.Names is null)
        //         return Result.Failure("Request doesn't contain any names by language code.");

        //     var value = string.Empty;
        //     var hasCurrentLanguageCode = marketRequest.Names.TryGetValue(languageCode, out value);
        //     var hasDefaultLanguageCode = marketRequest.Names.TryGetValue(LocalizationHelper.DefaultLanguageCode, out value);

        //     if (!hasCurrentLanguageCode && !hasDefaultLanguageCode)
        //         return Result.Failure("Request need to be contained at least current language code or default language code.");

        //     return Result.Success();
        // }


        private const int UnknownMarketId = 1;

        private readonly EdoContext _context;
        private readonly IMarketManagementStorage _marketStorage;
        private readonly ICountryManagementStorage _countryStorage;
    }
}