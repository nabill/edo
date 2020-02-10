using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Authorization;

namespace HappyTravel.Edo.Api.Filters.Authorization
{
    public class InCompanyPermissionsAuthorizeAttribute : AuthorizeAttribute
    {
        public InCompanyPermissionsAuthorizeAttribute(InCompanyPermissions permissions)
        {
            Policy = $"{PolicyPrefix}{permissions}";
        }
        
        public const string PolicyPrefix = "Customer_";
    }
}