using System;

namespace HappyTravel.Edo.Api.Services.Management
{
    [Flags]
    public enum GlobalPermissions
    {
        CompanyVerification = 1,
        CreditLimitChange = 2
    }
}