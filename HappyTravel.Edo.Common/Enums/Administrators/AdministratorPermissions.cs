using System;

namespace HappyTravel.Edo.Common.Enums.Administrators
{
    [Flags]
    public enum AdministratorPermissions
    {
        AdministratorInvitation = 1,
        AgencyVerification = 2,
        AccountReplenish = 4,
        MarkupManagement = 8,
        OfflinePayment = 16,
        AgencyBalanceObservation = 32,
        AgencyBalanceReplenishAndSubtract = 64,
        AgencyManagement = 256,
        PaymentLinkGeneration = 512,
        AccommodationsMerge = 1024,
        BookingManagement = 2048,
        AgentManagement = 4096,
        BalanceManualCorrection = 8192,
        BookingReportGeneration = 16384,
        FinanceReportGeneration = 32768,
        MarketingReportGeneration = 65536,
        AdministratorManagement = 131072,
        AccommodationsManagement = 262144,
        LocationsManagement = 524288,
        ViewAgencies = 1048576,
        ViewAgents = 2097152,
        AdministratorRoleManagement = 4194304,
        AdministratorNotificationManagement = 8388608,
        SupplierManagement = 16777216,
        ViewBookings = 33554432,
        CompanyAccountManagement = 67108864,
        ViewPaxNames = 134217728
    }
}