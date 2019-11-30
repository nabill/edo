using System.Collections.Generic;
using HappyTravel.EdoContracts.Accommodations.Internals;
using Newtonsoft.Json;
using SlimAccommodationDetails = HappyTravel.Edo.Api.Models.Accommodations.SlimAccommodationDetails;

namespace HappyTravel.Edo.Api.Models.Availabilities
{
    public readonly struct SlimAvailabilityResult
    {
        [JsonConstructor]
        public SlimAvailabilityResult(SlimAccommodationDetails accommodationDetails, List<Agreement> agreements, bool isPromo)
        {
            AccommodationDetails = accommodationDetails;
            Agreements = agreements;
            IsPromo = isPromo;
        }
        
        public SlimAvailabilityResult(SlimAvailabilityResult availabilityResult, List<Agreement> agreements)
        {
            AccommodationDetails = availabilityResult.AccommodationDetails;
            Agreements = agreements;
            IsPromo = availabilityResult.IsPromo;
        }


        public SlimAccommodationDetails AccommodationDetails { get; }
        public List<Agreement> Agreements { get; }
        public bool IsPromo { get; }
    }
}
