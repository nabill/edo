using HappyTravel.Edo.Api.Models.Management.Enums;
using Microsoft.AspNetCore.Authorization;

namespace HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters
{
    public class AdministratorPermissionsAuthorizationRequirement : IAuthorizationRequirement
    {
        public AdministratorPermissionsAuthorizationRequirement(AdministratorPermissions administratorPermissions)
        {
            AdministratorPermissions = administratorPermissions;
        }


        public AdministratorPermissions AdministratorPermissions { get; }
    }
}