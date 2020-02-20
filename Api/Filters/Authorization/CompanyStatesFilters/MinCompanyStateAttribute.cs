using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Authorization;

namespace HappyTravel.Edo.Api.Filters.Authorization.CompanyStatesFilters
{
    public class MinCompanyStateAttribute : AuthorizeAttribute
    {
        public MinCompanyStateAttribute(CompanyStates minimalState)
        {
            Policy = $"{PolicyPrefix}{minimalState}";
        }
        
        public const string PolicyPrefix = "MinCompanyState_";
    }
}