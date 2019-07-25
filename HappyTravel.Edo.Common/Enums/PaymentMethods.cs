using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PaymentMethods
    {
        BankTransfer = 0,
        CreditCard = 1,
        Cash = 2
    }
}