using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AccountEventType
    {
        None = 0,
        Add = 1,
        Charge = 2,
        Authorize = 3,
        Capture = 4,
        Void = 5,
        CounterpartyAdd = 6,
        CounterpartySubtract = 7,
        CounterpartyTransferToAgency = 8,
        AgencyTransferToAgency = 9,
        Refund = 10,
        ManualIncrease = 11,
        ManualDecrease = 12,
        AgencyAdd = 12,
        AgencySubtract = 13
    }
}
