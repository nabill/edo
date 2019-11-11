using System;

namespace HappyTravel.Edo.Common.Enums
{
    [Flags]
    public enum CustomerCompanyPermissions
    {
        None = 1,
        EditCompanyInfo = 2,
        PermissionManagement = 4,
        CustomerInvitation = 8,
        AccommodationAvailabilitySearch = 16,
        AccommodationBooking = 32
    }
}