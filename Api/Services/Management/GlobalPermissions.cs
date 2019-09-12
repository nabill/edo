using System;

namespace HappyTravel.Edo.Api.Services.Management
{
    [Flags]
    public enum GlobalPermissions
    {
        None = 0,
        CompanyVerification = 1,
        CreditLimitChange = 2
    }
}