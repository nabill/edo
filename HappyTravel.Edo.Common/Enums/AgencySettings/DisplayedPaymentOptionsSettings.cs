using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums.AgencySettings
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DisplayedPaymentOptionsSettings
    {
        CreditCardAndVirtualAccount = 0,
        CreditCard = 1,
    }
}
