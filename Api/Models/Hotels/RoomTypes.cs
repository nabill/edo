using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Api.Models.Hotels
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RoomTypes
    {
        NotSpecified
    }
}
