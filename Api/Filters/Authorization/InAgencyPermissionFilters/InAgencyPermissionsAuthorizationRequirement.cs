using Microsoft.AspNetCore.Authorization;

namespace HappyTravel.Edo.Api.Filters.Authorization.InAgencyPermissionFilters
{
    public class InAgencyPermissionsAuthorizationRequirement : IAuthorizationRequirement
    {
        public InAgencyPermissionsAuthorizationRequirement(Common.Enums.InAgencyPermissions permissions)
        {
            Permissions = permissions;
        }


        public Common.Enums.InAgencyPermissions Permissions { get; }
    }
}