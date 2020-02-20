using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Authorization;

namespace HappyTravel.Edo.Api.Filters.Authorization.CompanyStatesFilters
{
    public class MinCompanyStateAuthorizationRequirement : IAuthorizationRequirement
    {
        public MinCompanyStateAuthorizationRequirement(CompanyStates companyState)
        {
            CompanyState = companyState;
        }
        
        public CompanyStates CompanyState { get; }
    }
}