using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Services.CurrencyConversion;
using HappyTravel.Edo.Api.Services.Discounts;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Helpers;
using HappyTravel.Money.Models;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public class PriceProcessor : IPriceProcessor
    {
        public PriceProcessor(IMarkupService markupService, ICurrencyConverterService currencyConverter,
            IDiscountService discountService)
        {
            _markupService = markupService;
            _currencyConverter = currencyConverter;
            _discountService = discountService;
        }


        public Task<Result<TDetails, ProblemDetails>> ConvertCurrencies<TDetails>(AgentContext agent, TDetails details,
            Func<TDetails, PriceProcessFunction, ValueTask<TDetails>> changePricesFunc, Func<TDetails, Currencies?> getCurrencyFunc)
        {
            return _currencyConverter
                .ConvertPricesInData(agent, details, changePricesFunc, getCurrencyFunc)
                .ToResultWithProblemDetails();
        }


        public Task<TDetails> ApplyMarkups<TDetails>(AgentContext agent, TDetails details,
            Func<TDetails, PriceProcessFunction, ValueTask<TDetails>> priceProcessFunc,
            Action<MarkupApplicationResult<TDetails>> logAction = null)
            => _markupService.ApplyMarkups(agent, details, priceProcessFunc, logAction);
        
        
        public Task<TDetails> ApplyDiscounts<TDetails>(AgentContext agent, TDetails details,
            Func<TDetails, PriceProcessFunction, ValueTask<TDetails>> priceProcessFunc,
            Action<DiscountApplicationResult<TDetails>> logAction = null)
            => _discountService.ApplyDiscounts(agent, details, priceProcessFunc, logAction);


        private readonly IMarkupService _markupService;
        private readonly ICurrencyConverterService _currencyConverter;
        private readonly IDiscountService _discountService;
    }
}