using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Authorization;

namespace HappyTravel.Edo.Api.Filters.Authorization
{
    public class InCompanyPermissionsAuthorizationRequirement : IAuthorizationRequirement
    {
        public InCompanyPermissionsAuthorizationRequirement(InCompanyPermissions permissions)
        {
            Permissions = permissions;
        }


        public InCompanyPermissions Permissions { get; }
    }
}