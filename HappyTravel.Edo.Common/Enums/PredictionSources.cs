using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PredictionSources
    {
        NotSpecified = 0,
        [EnumMember(Value = "geocoder")]
        Google = 1,
        [EnumMember(Value = "netstorming-connector")]
        NetstormingConnector = 2
    }
}
