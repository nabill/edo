using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CounterpartyStates
    {
        PendingVerification = 0,
        FullAccess = 1,
        DeclinedVerification = 2,
        ReadOnly = 3
    }
}