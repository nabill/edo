using System.Collections.Generic;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Data.Markup;

namespace HappyTravel.Edo.Api.Services.Markups.Availability
{
    public readonly struct AvailabilityResponseWithMarkup 
    {
        public AvailabilityResponseWithMarkup(AvailabilityResponse supplierResponse, List<MarkupPolicy> appliedPolicies, AvailabilityResponse resultResponse)
        {
            SupplierResponse = supplierResponse;
            AppliedPolicies = appliedPolicies;
            ResultResponse = resultResponse;
        }
        
        public AvailabilityResponse SupplierResponse { get; }
        public List<MarkupPolicy> AppliedPolicies { get; }
        public AvailabilityResponse ResultResponse { get; }
    }
}