using Microsoft.AspNetCore.Authorization;

namespace HappyTravel.Edo.Api.Filters.Authorization.AgentExistingFilters
{
    public class AgentRequiredAttribute : AuthorizeAttribute
    {
        public AgentRequiredAttribute()
        {
            Policy = PolicyName;
        }

        public const string PolicyName = "AgentRequired";
    }
}