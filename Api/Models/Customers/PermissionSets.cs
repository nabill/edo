using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Customers
{
    public static class PermissionSets
    {
        public const InCompanyPermissions FullAccessDefault = 
            InCompanyPermissions.None | 
            InCompanyPermissions.AccommodationAvailabilitySearch |
            InCompanyPermissions.AccommodationBooking |
            InCompanyPermissions.CustomerInvitation; // 56

        public const InCompanyPermissions FullAccessMaster =
            InCompanyPermissions.All;

        public const InCompanyPermissions ReadOnlyDefault = 
            InCompanyPermissions.None | 
            InCompanyPermissions.AccommodationAvailabilitySearch | 
            InCompanyPermissions.CustomerInvitation; // 24

        public const InCompanyPermissions ReadOnlyMaster = 
            InCompanyPermissions.None | 
            InCompanyPermissions.AccommodationAvailabilitySearch | 
            InCompanyPermissions.CustomerInvitation | 
            InCompanyPermissions.EditCompanyInfo | 
            InCompanyPermissions.PermissionManagement; // 30
    }
}
