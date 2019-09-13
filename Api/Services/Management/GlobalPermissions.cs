using System;

namespace HappyTravel.Edo.Api.Services.Management
{
    [Flags]
    public enum GlobalPermissions
    {
        None = 1,
        CompanyVerification = 2,
        CreditLimitChange = 4
    }
}