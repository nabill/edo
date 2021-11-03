using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Authorization;

namespace HappyTravel.Edo.Api.Filters.Authorization.AgencyVerificationStatesFilters
{
    public class MinAgencyVerificationStateAuthorizationRequirement : IAuthorizationRequirement
    {
        public MinAgencyVerificationStateAuthorizationRequirement(AgencyVerificationStates agencyVerificationState)
        {
            AgencyVerificationState = agencyVerificationState;
        }
        
        public AgencyVerificationStates AgencyVerificationState { get; }
    }
}