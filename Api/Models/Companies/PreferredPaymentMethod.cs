using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Api.Models.Companies
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PreferredPaymentMethod
    {
        Сash,
        Сashless,
        CreditCard,
        DigitalСurrency
    }
}