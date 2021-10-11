using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
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


        public Task<Result<TDetails, ProblemDetails>> ConvertCurrencies<TDetails>(AgentContext agent, TDetails details,
            Func<TDetails, PriceProcessFunction, ValueTask<TDetails>> changePricesFunc, Func<TDetails, Currencies?> getCurrencyFunc)
        {
            return _currencyConverter
                .ConvertPricesInData(agent, details, changePricesFunc, getCurrencyFunc)
                .ToResultWithProblemDetails();
        }


        public Task<TDetails> ApplyMarkups<TDetails>(AgentContext agent, TDetails details,
            Func<TDetails, PriceProcessFunction, ValueTask<TDetails>> priceProcessFunc,
            Func<TDetails, MarkupObjectInfo> getMarkupObjectFunc = null,
            Action<MarkupApplicationResult<TDetails>> logAction = null)
        {
            var markupSubject = new MarkupSubjectInfo
            {
                AgentId = agent.AgentId,
                AgencyId = agent.AgencyId,
                CounterpartyId = agent.CounterpartyId
            };
            // TODO: Implement getting markup object for all models used with this function (TDetails)
            // https://github.com/happy-travel/agent-app-project/issues/696
            var markupObject = getMarkupObjectFunc?.Invoke(details) ?? default;
            
            return _markupService.ApplyMarkups(markupSubject, markupObject, details, priceProcessFunc, logAction);
        }


        private readonly IMarkupService _markupService;
        private readonly ICurrencyConverterService _currencyConverter;
    }
}