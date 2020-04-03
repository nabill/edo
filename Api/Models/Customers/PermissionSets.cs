using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Customers
{
    public static class PermissionSets
    {
        public const InCounterpartyPermissions FullAccessDefault = 
            InCounterpartyPermissions.None | 
            InCounterpartyPermissions.AccommodationAvailabilitySearch |
            InCounterpartyPermissions.AccommodationBooking |
            InCounterpartyPermissions.CustomerInvitation; // 56

        public const InCounterpartyPermissions FullAccessMaster =
            InCounterpartyPermissions.All;

        public const InCounterpartyPermissions ReadOnlyDefault = 
            InCounterpartyPermissions.None | 
            InCounterpartyPermissions.AccommodationAvailabilitySearch | 
            InCounterpartyPermissions.CustomerInvitation; // 24

        public const InCounterpartyPermissions ReadOnlyMaster = 
            InCounterpartyPermissions.None | 
            InCounterpartyPermissions.AccommodationAvailabilitySearch | 
            InCounterpartyPermissions.CustomerInvitation | 
            InCounterpartyPermissions.EditCounterpartyInfo | 
            InCounterpartyPermissions.PermissionManagementInCounterparty; // 30
    }
}
