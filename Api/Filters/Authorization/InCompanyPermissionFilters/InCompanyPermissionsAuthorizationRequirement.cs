using Microsoft.AspNetCore.Authorization;

namespace HappyTravel.Edo.Api.Filters.Authorization.InCompanyPermissionFilters
{
    public class InCompanyPermissionsAuthorizationRequirement : IAuthorizationRequirement
    {
        public InCompanyPermissionsAuthorizationRequirement(Common.Enums.InCompanyPermissions permissions)
        {
            Permissions = permissions;
        }


        public Common.Enums.InCompanyPermissions Permissions { get; }
    }
}