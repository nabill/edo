using Microsoft.AspNetCore.Authorization;

namespace HappyTravel.Edo.Api.Filters.Authorization.InCompanyPermissionFilters
{
    public class InCompanyPermissionsAuthorizeAttribute : AuthorizeAttribute
    {
        public InCompanyPermissionsAuthorizeAttribute(Common.Enums.InCompanyPermissions permissions)
        {
            Policy = $"{PolicyPrefix}{permissions}";
        }
        
        public const string PolicyPrefix = "InCompanyPermissions_";
    }
}