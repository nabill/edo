using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Api.Models.Availabilities
{
    [JsonConverter(typeof(StringEnumConverter))]
    [Flags]
    public enum PropertyTypes
    {
        Hotels = 1,
        Apartments = 2
    }
}
