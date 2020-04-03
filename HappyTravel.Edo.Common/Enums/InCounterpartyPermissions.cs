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
        PermissionManagementInBranch = 128,
        ObserveMarkupInCounterparty = 256,
        ObserveMarkupInBranch = 512,
        // "All" permission level should be recalculated after adding new permission
        All = EditCounterpartyInfo | 
            PermissionManagementInCounterparty | 
            CustomerInvitation | 
            AccommodationAvailabilitySearch | 
            AccommodationBooking |
            ViewCounterpartyAllPaymentHistory |
            PermissionManagementInBranch |
            ObserveMarkupInCounterparty |
            ObserveMarkupInBranch // 1022
    }
}