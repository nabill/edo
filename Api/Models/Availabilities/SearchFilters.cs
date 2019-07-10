using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Api.Models.Availabilities
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SearchFilters
    {
        Default
    }
}
