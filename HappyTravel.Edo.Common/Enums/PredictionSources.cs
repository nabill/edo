using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PredictionSources
    {
        NotSpecified = 0,
        Google = 1,
        NetstormingConnector = 2
    }
}
