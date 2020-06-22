using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public static class PermissionSets
    {
        public const InAgencyPermissions FullAccessDefault = 
            InAgencyPermissions.None | 
            InAgencyPermissions.AccommodationAvailabilitySearch |
            InAgencyPermissions.AccommodationBooking |
            InAgencyPermissions.AgentInvitation; // 15

        public const InAgencyPermissions FullAccessMaster =
            InAgencyPermissions.All;

        public const InAgencyPermissions ReadOnlyDefault = 
            InAgencyPermissions.None | 
            InAgencyPermissions.AccommodationAvailabilitySearch | 
            InAgencyPermissions.AgentInvitation; // 7

        public const InAgencyPermissions ReadOnlyMaster =
            InAgencyPermissions.None |
            InAgencyPermissions.AccommodationAvailabilitySearch |
            InAgencyPermissions.AgentInvitation |
            InAgencyPermissions.PermissionManagement |
            InAgencyPermissions.ObserveAgents;  // 87
    }
}
