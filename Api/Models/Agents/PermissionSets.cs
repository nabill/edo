using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public static class PermissionSets
    {
        public const InCounterpartyPermissions FullAccessDefault = 
            InCounterpartyPermissions.None | 
            InCounterpartyPermissions.AccommodationAvailabilitySearch |
            InCounterpartyPermissions.AccommodationBooking |
            InCounterpartyPermissions.AgentInvitation; // 56

        public const InCounterpartyPermissions FullAccessMaster =
            InCounterpartyPermissions.All;

        public const InCounterpartyPermissions ReadOnlyDefault = 
            InCounterpartyPermissions.None | 
            InCounterpartyPermissions.AccommodationAvailabilitySearch | 
            InCounterpartyPermissions.AgentInvitation; // 24

        public const InCounterpartyPermissions ReadOnlyMaster = 
            InCounterpartyPermissions.None | 
            InCounterpartyPermissions.AccommodationAvailabilitySearch | 
            InCounterpartyPermissions.AgentInvitation | 
            InCounterpartyPermissions.EditCounterpartyInfo | 
            InCounterpartyPermissions.PermissionManagementInCounterparty; // 30
    }
}
