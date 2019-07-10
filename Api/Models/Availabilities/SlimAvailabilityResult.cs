using System.Collections.Generic;
using HappyTravel.Edo.Api.Models.Hotels;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Availabilities
{
    public readonly struct SlimAvailabilityResult
    {
        [JsonConstructor]
        public SlimAvailabilityResult(SlimHotelDetails hotelDetails, List<RichAgreement> agreements, bool isPromo)
        {
            HotelDetails = hotelDetails;
            Agreements = agreements;
            IsPromo = isPromo;
        }


        public SlimHotelDetails HotelDetails { get; }
        public List<RichAgreement> Agreements { get; }
        public bool IsPromo { get; }
    }
}
