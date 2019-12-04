using System.Globalization;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Infrastructure.Formatters
{
    public static class PaymentAmountFormatter
    {
        public static string ToCurrencyString(decimal amount, Currencies currency)
        {
            switch (currency)
            {
                case Currencies.USD:
                    return string.Format(new CultureInfo("en-US"), "{0:C}", amount);
                case Currencies.EUR:
                    return string.Format(new CultureInfo("de-DE"), "{0:C}", amount);
                case Currencies.AED:
                    return string.Format(new CultureInfo("ar-SA"), "{0:C}", amount);
                case Currencies.SAR:
                    return string.Format(new CultureInfo("ar-AE"), "{0:C}", amount);
                default:
                case Currencies.NotSpecified:
                    return $"{amount:F2}";
            }
        }
    }
}
