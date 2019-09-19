using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UserType
    {
        NotSpecified = 0,
        Admin = 1,
        Customer = 2,
    }
}
