using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Api.Models.Availabilities
{
    [JsonConverter(typeof (StringEnumConverter))]
    [Flags]
    public enum ClientSearchFilters
    {
        AvailableOnly = 1,
        AvailableWeighted = 2,
        BestPrice = 4,
        BestContract = 8,
        BestRoomPlans = 16,
        ExcludeNonRefundable = 32,
        ExcludeDynamic = 64,
        BestArrangement = 128,
    }
}