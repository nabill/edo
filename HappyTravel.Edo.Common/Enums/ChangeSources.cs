using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChangeSources
    {
        None = 0,
        Supplier = 1,
        System = 2,
        Administrator = 3
    }
}
