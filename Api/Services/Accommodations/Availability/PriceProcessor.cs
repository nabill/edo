using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.CurrencyConversion;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Api.Services.Markups.Abstractions;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.Money.Enums;
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


        public Task<Result<TDetails, ProblemDetails>> ConvertCurrencies<TDetails>(TDetails details,
            Func<TDetails, PriceProcessFunction, ValueTask<TDetails>> changePricesFunc, Func<TDetails, Currencies?> getCurrencyFunc)
        {
            return _currencyConverter
                .ConvertPricesInData(details, changePricesFunc, getCurrencyFunc)
                .ToResultWithProblemDetails();
        }


        public async Task<TDetails> ApplyMarkups<TDetails>(MarkupSubjectInfo subjectInfo, TDetails details,
            Func<TDetails, PriceProcessFunction, ValueTask<TDetails>> priceProcessFunc,
            Func<TDetails, MarkupDestinationInfo> getMarkupDestinationFunc,
            Action<MarkupApplicationResult<TDetails>> logAction = null)
        {
            var markupDestination = getMarkupDestinationFunc(details);

            return await _markupService.ApplyMarkups(subjectInfo, markupDestination, details, priceProcessFunc, logAction);
        }


        private readonly IMarkupService _markupService;
        private readonly ICurrencyConverterService _currencyConverter;
    }
}