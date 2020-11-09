using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum InvoiceStatuses
    {
        Actual = 0,
        Cancelled = 1
    }
}