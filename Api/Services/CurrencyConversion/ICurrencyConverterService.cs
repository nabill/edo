using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.EdoContracts.General.Enums;

namespace HappyTravel.Edo.Api.Services.CurrencyConversion
{
    public interface ICurrencyConverterService
    {
        Task<Result<TDetails>> ConvertPricesInData<TDetails>(CustomerInfo customer, TDetails data,
            Func<TDetails, PriceProcessFunction, ValueTask<TDetails>> changePricesFunc, Func<TDetails, Currencies> getCurrencyFunc);
    }
}