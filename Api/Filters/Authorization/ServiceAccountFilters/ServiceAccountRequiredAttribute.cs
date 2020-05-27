using Microsoft.AspNetCore.Authorization;

namespace HappyTravel.Edo.Api.Filters.Authorization.ServiceAccountFilters
{
    public class ServiceAccountRequiredAttribute : AuthorizeAttribute
    {
        public ServiceAccountRequiredAttribute()
        {
            Policy = PolicyName;
        }

        public const string PolicyName = "ServiceAccountRequired";
    }
}