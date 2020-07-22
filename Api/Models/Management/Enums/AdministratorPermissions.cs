using System;

namespace HappyTravel.Edo.Api.Models.Management.Enums
{
    [Flags]
    public enum AdministratorPermissions
    {
        None = 1,
        AdministratorInvitation = 2,
        CounterpartyVerification = 4,
        CreditLimitChange = 8,
        AccountReplenish = 16,
        MarkupManagement = 32,
        OfflinePayment = 64,
        CounterpartyBalanceObservation = 128,
        CounterpartyBalanceReplenishAndSubtract = 256,
        CounterpartyToAgencyTransfer = 512,
        CounterpartyManagement = 1024,
        PaymentLinkGeneration =2048,
    }
}