using System;
using System.Collections.Generic;
using HappyTravel.EdoContracts.General.Enums;

namespace HappyTravel.Edo.Api.Infrastructure.Money
{
    internal static class MoneyRounder
    {
        public static decimal Round(decimal sourceValue, Currencies currency, MidpointRounding rounding = MidpointRounding.AwayFromZero) => Math.Round(sourceValue, CurrencyDecimalDigits[currency], rounding);

        private static readonly Dictionary<Currencies, int> CurrencyDecimalDigits = new Dictionary<Currencies, int>
        {
            {Currencies.AED, 2},
            {Currencies.EUR, 2},
            {Currencies.SAR, 2},
            {Currencies.USD, 2}
        };
    }
}