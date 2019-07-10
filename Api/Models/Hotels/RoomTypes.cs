using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Api.Models.Hotels
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RoomTypes
    {
        NotSpecified,
        Single,
        TwinOrSingle,
        Twin,
        Double,
        Triple,
        Quadruple,
        /// <summary>
        /// Family Room (2 adult + 2 extra bed)
        /// </summary>
        Family
    }
}
