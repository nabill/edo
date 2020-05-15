using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public static class PermissionSets
    {
        public const InAgencyPermissions FullAccessDefault = 
            InAgencyPermissions.None | 
            InAgencyPermissions.AccommodationAvailabilitySearch |
            InAgencyPermissions.AccommodationBooking |
            InAgencyPermissions.AgentInvitation; // 56

        public const InAgencyPermissions FullAccessMaster =
            InAgencyPermissions.All;

        public const InAgencyPermissions ReadOnlyDefault = 
            InAgencyPermissions.None | 
            InAgencyPermissions.AccommodationAvailabilitySearch | 
            InAgencyPermissions.AgentInvitation; // 24

        public const InAgencyPermissions ReadOnlyMaster = 
            InAgencyPermissions.None | 
            InAgencyPermissions.AccommodationAvailabilitySearch | 
            InAgencyPermissions.AgentInvitation | 
            InAgencyPermissions.EditCounterpartyInfo | 
            InAgencyPermissions.PermissionManagementInCounterparty; // 30
    }
}
