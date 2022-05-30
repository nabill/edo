using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    [Flags]
    public enum InAgencyPermissions
    {
        // Need to create migration after changes!
        // View RemoveDefaultValueFromInAgencyPermissions for example
        AgentInvitation = 1,
        AccommodationAvailabilitySearch = 2,
        AccommodationBooking = 4,
        PermissionManagement = 8,
        ObserveMarkup = 16,
        ObserveAgents = 32,
        ObserveBalance = 64,
        ObservePaymentHistory = 128,
        AgencyToChildTransfer = 256,
        ReceiveBookingSummary = 512,
        AgencyBookingsManagement = 1024,
        AgentStatusManagement = 2048,
        ObserveAgencyInvitations = 4096,
        AgencyImagesManagement = 8192,
        ObserveAgencyContract = 16384,
        MarkupManagement = 32768,
        ObserveChildAgencies = 65536,
        InviteChildAgencies = 131072,
        ApiConnectionManagement = 262144,
        // All = 01111111111111111111111111111111. First bit is 0 because it is reserved for sign
        All = 2147483647
    }
}