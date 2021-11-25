using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Services.Markups.Abstractions;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.Money.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public interface IPriceProcessor
    {
        Task<Result<TDetails, ProblemDetails>> ConvertCurrencies<TDetails>(TDetails details, Func<TDetails, PriceProcessFunction, ValueTask<TDetails>> changePricesFunc, Func<TDetails, Currencies?> getCurrencyFunc);


        Task<TDetails> ApplyMarkups<TDetails>(MarkupSubjectInfo subjectInfo, TDetails details,
            Func<TDetails, PriceProcessFunction, ValueTask<TDetails>> priceProcessFunc,
            Func<TDetails, MarkupDestinationInfo> getMarkupDestinationFunc = null,
            Action<MarkupApplicationResult<TDetails>> logAction = null);
    }
}