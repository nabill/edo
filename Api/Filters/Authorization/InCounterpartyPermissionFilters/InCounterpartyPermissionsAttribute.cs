using Microsoft.AspNetCore.Authorization;

namespace HappyTravel.Edo.Api.Filters.Authorization.InCounterpartyPermissionFilters
{
    public class InCounterpartyPermissionsAttribute : AuthorizeAttribute
    {
        public InCounterpartyPermissionsAttribute(Common.Enums.InCounterpartyPermissions permissions)
        {
            Policy = $"{PolicyPrefix}{permissions}";
        }
        
        public const string PolicyPrefix = "InCompanyPermissions_";
    }
}