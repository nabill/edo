using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.EdoContracts.General.Enums;

namespace HappyTravel.Edo.Api.Services.CurrencyConversion
{
    public class CurrencyConverterService : ICurrencyConverterService
    {
        public CurrencyConverterService(ICurrencyRateService rateService,
            ICustomerSettingsManager customerSettingsManager,
            ICounterpartyService counterpartyService,
            IMemoryFlow memoryFlow)
        {
            _rateService = rateService;
            _customerSettingsManager = customerSettingsManager;
            _counterpartyService = counterpartyService;
            _memoryFlow = memoryFlow;
        }


        public async Task<Result<TData>> ConvertPricesInData<TData>(CustomerInfo customer, TData data,
            Func<TData, PriceProcessFunction, ValueTask<TData>> changePricesFunc, Func<TData, Currencies?> getCurrencyFunc)
        {
            var currentCurrency = getCurrencyFunc(data);
            if(!currentCurrency.HasValue)
                return Result.Ok(data);
                
            if (currentCurrency == Currencies.NotSpecified)
                return Result.Fail<TData>($"Cannot convert from '{Currencies.NotSpecified}' currency");
            
            var targetCurrency = await GetTargetCurrency(customer);
            if (targetCurrency == Currencies.NotSpecified)
                return Result.Fail<TData>($"Cannot convert to '{Currencies.NotSpecified}' currency");

            if (targetCurrency == currentCurrency)
                return Result.Ok(data);
            
            var (_, isFailure, rate, error) = await _rateService.Get(currentCurrency.Value, targetCurrency);
            if (isFailure)
                return Result.Fail<TData>(error);

            var convertedDetails = await changePricesFunc(data, (price, currency) =>
            {
                // TODO: Add more complex logic conversion logic with ceiling, rounding and other.
                var newPrice = price * rate;
                var newCurrency = targetCurrency;

                return new ValueTask<(decimal, Currencies)>((newPrice, newCurrency));
            });
            
            return Result.Ok(convertedDetails);
            
            ValueTask<Currencies> GetTargetCurrency(CustomerInfo customerInfo)
            {
                var key = _memoryFlow.BuildKey(nameof(CurrencyConverterService), "TARGET_CURRENCY", customerInfo.CustomerId.ToString());
                return _memoryFlow.GetOrSetAsync(key, async () =>
                {
                    var settings = await _customerSettingsManager.GetUserSettings(customerInfo);
                    if (settings.DisplayCurrency != Currencies.NotSpecified)
                        return settings.DisplayCurrency;

                    var (_, _, counterparty, _) = await _counterpartyService.Get(customerInfo.CounterpartyId);
                    return counterparty.PreferredCurrency;
                }, TargetCurrencyCacheLifeTime);
            }
        }


        private static readonly TimeSpan TargetCurrencyCacheLifeTime = TimeSpan.FromMinutes(10);

        private readonly ICurrencyRateService _rateService;
        private readonly ICustomerSettingsManager _customerSettingsManager;
        private readonly ICounterpartyService _counterpartyService;
        private readonly IMemoryFlow _memoryFlow;
    }
}