using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums.AgencySettings
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AprMode
    {
        Hide = 0,
        DisplayOnly = 1,
        CardPurchasesOnly = 2,
        CardAndAccountPurchases = 3
    }
}
