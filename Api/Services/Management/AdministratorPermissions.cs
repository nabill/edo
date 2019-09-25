using System;

namespace HappyTravel.Edo.Api.Services.Management
{
    [Flags]
    public enum AdministratorPermissions
    {
        None = 1,
        CompanyVerification = 2,
        CreditLimitChange = 4,
        AccountReplenish = 8,
        MarkupManagement = 16
    }
}