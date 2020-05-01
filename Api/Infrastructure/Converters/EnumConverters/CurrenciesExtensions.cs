using System.ComponentModel;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Infrastructure.Converters.EnumConverters
{
    public static class CurrenciesExtensions
    {
        internal static string ToCurrencyCode(Currencies currency)
        {
            switch (currency)
            {
                case Currencies.AED:
                    return "AED";
                case Currencies.SAR:
                    return  "SAR";
                case Currencies.USD:
                    return  "USD";
                case Currencies.EUR:
                    return "EUR";
                case Currencies.NotSpecified:
                    return "NotSpecified";
                default:
                    throw new InvalidEnumArgumentException($"Unknown currency: {currency}");
            }
        }
    }
}