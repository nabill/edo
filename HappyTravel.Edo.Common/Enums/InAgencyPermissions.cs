using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    [Flags]
    public enum InAgencyPermissions
    {
        None = 1,
        AgentInvitation = 2,
        AccommodationAvailabilitySearch = 4,
        AccommodationBooking = 8,
        PermissionManagement = 16,
        ObserveMarkup = 32,
        ObserveAgents = 64,
        ObserveBalance = 128,
        ObservePaymentHistory = 256,
        AgencyToChildTransfer = 512,
        ReceiveBookingSummary = 1024,
        AgencyBookingsManagement = 2048,
        // "All" permission level should be recalculated after adding new permission
        All = 
            AgentInvitation | 
            AccommodationAvailabilitySearch | 
            AccommodationBooking |
            PermissionManagement |
            ObserveMarkup |
            ObserveAgents |
            ObserveBalance |
            ObservePaymentHistory |
            AgencyToChildTransfer |
            ReceiveBookingSummary |
            AgencyBookingsManagement // 4094
    }
}