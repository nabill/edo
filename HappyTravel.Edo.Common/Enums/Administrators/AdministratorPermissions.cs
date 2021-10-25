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
        CounterpartyBalanceObservation = 32,
        CounterpartyBalanceReplenishAndSubtract = 64,
        CounterpartyToAgencyTransfer = 128,
        CounterpartyManagement = 256,
        PaymentLinkGeneration = 512,
        AccommodationDuplicatesReportApproval = 1024,
        BookingManagement = 2048,
        AgentManagement = 4096,
        BalanceManualCorrection = 8192,
        BookingReportGeneration = 16384,
        AccountsReportGeneration = 32768,
        CompanyReportGeneration = 65536,
        AdministratorManagement = 131072,
        MapperAccommodationManagement = 33554432
    }
}