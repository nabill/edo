using HappyTravel.Edo.Common.Enums.Administrators;
using Microsoft.AspNetCore.Authorization;

namespace HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters
{
    public class AdministratorPermissionsAttribute : AuthorizeAttribute
    {
        public AdministratorPermissionsAttribute(AdministratorPermissions administratorPermissions)
        {
            Policy = $"{PolicyPrefix}{administratorPermissions}";
        }

        public const string PolicyPrefix = "AdministratorsPermission_";
    }
}