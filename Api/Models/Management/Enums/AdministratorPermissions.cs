using System;

namespace HappyTravel.Edo.Api.Models.Management.Enums
{
    [Flags]
    public enum AdministratorPermissions
    {
        None = 1,
        AdministratorInvitation = 2,
        CounterpartyVerification = 4,
        AccountReplenish = 8,
        MarkupManagement = 16,
        OfflinePayment = 32,
        CounterpartyBalanceObservation = 64,
        CounterpartyBalanceReplenishAndSubtract = 128,
        CounterpartyToAgencyTransfer = 256,
        CounterpartyManagement = 512,
        PaymentLinkGeneration = 1024,
        AccommodationDuplicatesReportApproval = 2048,
        BoookingCancellation = 4096,
        AgentManagement = 8192,
        BalanceManualCorrection = 16184
    }
}