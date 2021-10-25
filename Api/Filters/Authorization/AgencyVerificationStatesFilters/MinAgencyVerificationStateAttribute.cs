using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Authorization;

namespace HappyTravel.Edo.Api.Filters.Authorization.AgencyVerificationStatesFilters
{
    public class MinAgencyVerificationStateAttribute : AuthorizeAttribute
    {
        public MinAgencyVerificationStateAttribute(AgencyVerificationStates minimalState)
        {
            Policy = $"{PolicyPrefix}{minimalState}";
        }
        
        public const string PolicyPrefix = "MinAgencyVerificationState_";
    }
}