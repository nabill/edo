using Microsoft.AspNetCore.Authorization;

namespace HappyTravel.Edo.Api.Filters.Authorization.InAgencyPermissionFilters
{
    public class InAgencyPermissionsAttribute : AuthorizeAttribute
    {
        public InAgencyPermissionsAttribute(Common.Enums.InAgencyPermissions permissions)
        {
            Policy = $"{PolicyPrefix}{permissions}";
        }
        
        public const string PolicyPrefix = "InAgencyPermissions_";
    }
}