using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Data.AccommodationMappings
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AccommodationDuplicateReportState
    {
        PendingApproval = 1,
        Approved = 2,
        Disapproved = 3
    }
}