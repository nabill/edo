using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Api.Models.Management.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ActivityStatus
    {
        Active,
        NotActive
    }
}