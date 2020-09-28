using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Services.CurrencyConversion;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public class PriceProcessor : IPriceProcessor
    {
        public PriceProcessor(IMarkupService markupService, ICurrencyConverterService currencyConverter)
        {
            _markupService = markupService;
            _currencyConverter = currencyConverter;
        }


        public Task<Result<TDetails, ProblemDetails>> ConvertCurrencies<TDetails>(AgentContext agent, TDetails details,
            Func<TDetails, PriceProcessFunction, ValueTask<TDetails>> changePricesFunc, Func<TDetails, Currencies?> getCurrencyFunc)
        {
            return _currencyConverter
                .ConvertPricesInData(agent, details, changePricesFunc, getCurrencyFunc)
                .ToResultWithProblemDetails();
        }


        public async Task<DataWithMarkup<TDetails>> ApplyMarkups<TDetails>(AgentContext agent, TDetails details,
            Func<TDetails, PriceProcessFunction, ValueTask<TDetails>> priceProcessFunc)
        {
            var markup = await _markupService.Get(agent, MarkupPolicyTarget.AccommodationAvailability);
            var responseWithMarkup = await priceProcessFunc(details, markup.Function);
            var ceiledResponse = await priceProcessFunc(responseWithMarkup, (price, currency) =>
            {
                var roundedPrice = MoneyRounder.Ceil(price, currency);
                return new ValueTask<(decimal, Currencies)>((roundedPrice, currency));
            });

            return DataWithMarkup.Create(ceiledResponse, markup.Policies);
        }


        private readonly ICurrencyConverterService _currencyConverter;

        private readonly IMarkupService _markupService;
    }
}