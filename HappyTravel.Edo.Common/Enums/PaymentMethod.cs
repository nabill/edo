using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PaymentMethod
    {
        Cash,
        Cashless,
        CreditCard,
        DigitalCurrency
    }
}