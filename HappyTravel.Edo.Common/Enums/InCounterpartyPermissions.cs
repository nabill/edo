using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    [Flags]
    public enum InCounterpartyPermissions
    {
        None = 1,
        EditCounterpartyInfo = 2,
        PermissionManagementInCounterparty = 4,
        CustomerInvitation = 8,
        AccommodationAvailabilitySearch = 16,
        AccommodationBooking = 32,
        ViewCounterpartyAllPaymentHistory = 64,
        PermissionManagementInAgency = 128,
        ObserveMarkupInCounterparty = 256,
        ObserveMarkupInAgency = 512,
        // "All" permission level should be recalculated after adding new permission
        All = EditCounterpartyInfo | 
            PermissionManagementInCounterparty | 
            CustomerInvitation | 
            AccommodationAvailabilitySearch | 
            AccommodationBooking |
            ViewCounterpartyAllPaymentHistory |
            PermissionManagementInAgency |
            ObserveMarkupInCounterparty |
            ObserveMarkupInAgency // 1022
    }
}