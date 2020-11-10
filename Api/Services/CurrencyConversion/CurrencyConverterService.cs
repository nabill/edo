using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Helpers;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Services.CurrencyConversion
{
    public class CurrencyConverterService : ICurrencyConverterService
    {
        public CurrencyConverterService(ICurrencyRateService rateService)
        {
            _rateService = rateService;
        }


        public async Task<Result<TData>> ConvertPricesInData<TData>(AgentContext agent, TData data,
            Func<TData, PriceProcessFunction, ValueTask<TData>> changePricesFunc, Func<TData, Currencies?> getCurrencyFunc)
        {
            var currentCurrency = getCurrencyFunc(data);
            if(!currentCurrency.HasValue)
                return Result.Success(data);
                
            if (currentCurrency == Currencies.NotSpecified)
                return Result.Failure<TData>($"Cannot convert from '{Currencies.NotSpecified}' currency");
            
            if (currentCurrency == TargetCurrency)
                return Result.Success(data);
            
            var (_, isFailure, rate, error) = await _rateService.Get(currentCurrency.Value, TargetCurrency);
            if (isFailure)
                return Result.Failure<TData>(error);

            var convertedDetails = await changePricesFunc(data, price =>
            {
                var newAmount = price.Amount * rate * (1 + ConversionBuffer);
                var ceiledAmount = MoneyRounder.Ceil(newAmount, TargetCurrency);

                return new ValueTask<MoneyAmount>(new MoneyAmount(ceiledAmount, TargetCurrency));
            });
            
            return Result.Success(convertedDetails);
        }


        private const decimal ConversionBuffer = (decimal)0.005;
        
        // Only USD Currency is supported for now.
        private const Currencies TargetCurrency = Currencies.USD;

        private readonly ICurrencyRateService _rateService;
    }
}