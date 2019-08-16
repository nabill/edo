using System.Collections.Generic;
using HappyTravel.Edo.Api.Models.Accommodations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Availabilities
{
    public readonly struct SlimAvailabilityResult
    {
        [JsonConstructor]
        public SlimAvailabilityResult(SlimAccommodationDetails accommodationDetails, List<RichAgreement> agreements, bool isPromo)
        {
            AccommodationDetails = accommodationDetails;
            Agreements = agreements;
            IsPromo = isPromo;
        }


        public SlimAccommodationDetails AccommodationDetails { get; }
        public List<RichAgreement> Agreements { get; }
        public bool IsPromo { get; }
    }
}
