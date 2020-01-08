using System.Collections.Generic;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.Edo.Api.Models.Markups.Availability
{
    public readonly struct AvailabilityDetailsWithMarkup
    {
        public AvailabilityDetailsWithMarkup(List<MarkupPolicy> appliedPolicies, AvailabilityDetails resultResponse)
        {
            AppliedPolicies = appliedPolicies;
            ResultResponse = resultResponse;
        }


        public List<MarkupPolicy> AppliedPolicies { get; }
        public AvailabilityDetails ResultResponse { get; }
    }
}