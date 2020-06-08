using System;

namespace HappyTravel.Edo.Api.Models.Management.Enums
{
    [Flags]
    public enum AdministratorPermissions
    {
        None = 1,
        CounterpartyVerification = 2,
        CreditLimitChange = 4,
        AccountReplenish = 8,
        MarkupManagement = 16,
        OfflinePayment = 32,
        CounterpartyBalanceObservation = 64,
        CounterpartyBalanceReplanishAndSubtract = 128,
        CounterpartyToAgencyTransfer = 256
    }
}