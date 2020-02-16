using Microsoft.AspNetCore.Authorization;

namespace HappyTravel.Edo.Api.Filters.Authorization.InCompanyPermissionFilters
{
    public class InCompanyPermissionsAttribute : AuthorizeAttribute
    {
        public InCompanyPermissionsAttribute(Common.Enums.InCompanyPermissions permissions)
        {
            Policy = $"{PolicyPrefix}{permissions}";
        }
        
        public const string PolicyPrefix = "InCompanyPermissions_";
    }
}