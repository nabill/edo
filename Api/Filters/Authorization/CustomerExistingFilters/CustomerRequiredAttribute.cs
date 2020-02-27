using Microsoft.AspNetCore.Authorization;

namespace HappyTravel.Edo.Api.Filters.Authorization.CustomerExistingFilters
{
    public class CustomerRequiredAttribute : AuthorizeAttribute
    {
        public CustomerRequiredAttribute()
        {
            Policy = "CustomerRequired";
        }
    }
}