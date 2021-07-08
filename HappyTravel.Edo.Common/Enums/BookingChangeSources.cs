using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BookingChangeSources
    {
        None = 0,
        Administrator = 1,
        Supplier = 3,
        System = 4,
        PropertyOwner = 5
    }
}
