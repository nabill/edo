using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UserTypes
    {
        NotSpecified = 0,
        Admin = 1,
        Agent = 2,
        ServiceAccount = 3,
        InternalServiceAccount = 4
    }
}
