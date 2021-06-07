using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace HappyTravel.Edo.Notifications.Enums
{
    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProtocolTypes
    {
        None = 0,
        WebSocket = 1,
        Email = 2
    }
}