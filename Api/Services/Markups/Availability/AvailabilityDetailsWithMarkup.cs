using System.Collections.Generic;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.Edo.Api.Services.Markups.Availability
{
    public readonly struct AvailabilityDetailsWithMarkup
    {
        public AvailabilityDetailsWithMarkup(AvailabilityDetails supplierResponse, List<MarkupPolicy> appliedPolicies, AvailabilityDetails resultResponse)
        {
            AppliedPolicies = appliedPolicies;
            ResultResponse = resultResponse;
            SupplierResponse = supplierResponse;
        }


        public List<MarkupPolicy> AppliedPolicies { get; }
        public AvailabilityDetails ResultResponse { get; }
        public AvailabilityDetails SupplierResponse { get; }
    }
}