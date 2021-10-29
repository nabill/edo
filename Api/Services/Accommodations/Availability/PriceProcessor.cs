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
        public PriceProcessor(IMarkupService markupService, ICurrencyConverterService currencyConverter, IAgencyService agencyService)
        {
            _markupService = markupService;
            _currencyConverter = currencyConverter;
            _agencyService = agencyService;
        }


        public Task<Result<TDetails, ProblemDetails>> ConvertCurrencies<TDetails>(TDetails details,
            Func<TDetails, PriceProcessFunction, ValueTask<TDetails>> changePricesFunc, Func<TDetails, Currencies?> getCurrencyFunc)
        {
            return _currencyConverter
                .ConvertPricesInData(details, changePricesFunc, getCurrencyFunc)
                .ToResultWithProblemDetails();
        }


        public async Task<TDetails> ApplyMarkups<TDetails>(AgentContext agent, TDetails details,
            Func<TDetails, PriceProcessFunction, ValueTask<TDetails>> priceProcessFunc,
            Func<TDetails, MarkupObjectInfo> getMarkupObjectFunc,
            Action<MarkupApplicationResult<TDetails>> logAction = null)
        {
            var (_, isFailure, agency, error) = await _agencyService.Get(agent);
            if (isFailure)
                throw new Exception(error);
            
            var markupSubject = new MarkupSubjectInfo
            {
                AgentId = agent.AgentId,
                AgencyId = agent.AgencyId,
                CounterpartyId = agent.CounterpartyId,
                AgencyAncestors = agency.Ancestors,
                CountryHtId = agency.CountryHtId,
                LocalityHtId = agency.LocalityHtId
            };
            var markupObject = getMarkupObjectFunc(details);

            return await _markupService.ApplyMarkups(markupSubject, markupObject, details, priceProcessFunc, logAction);
        }


        private readonly IMarkupService _markupService;
        private readonly ICurrencyConverterService _currencyConverter;
        private readonly IAgencyService _agencyService;
    }
}