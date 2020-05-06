using Microsoft.AspNetCore.Authorization;

namespace HappyTravel.Edo.Api.Filters.Authorization.InCounterpartyPermissionFilters
{
    public class InCounterpartyPermissionsAuthorizationRequirement : IAuthorizationRequirement
    {
        public InCounterpartyPermissionsAuthorizationRequirement(Common.Enums.InCounterpartyPermissions permissions)
        {
            Permissions = permissions;
        }


        public Common.Enums.InCounterpartyPermissions Permissions { get; }
    }
}