using System.Collections.Generic;
using HappyTravel.Edo.Data.AccommodationMappings;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct ReportAccommodationDuplicateRequest
    {
        [JsonConstructor]
        public ReportAccommodationDuplicateRequest(ProviderAccommodationId accommodation, List<ProviderAccommodationId> duplicates)
        {
            Accommodation = accommodation;
            Duplicates = duplicates;
        }
        
        /// <summary>
        /// Reported accommodation.
        /// </summary>
        public ProviderAccommodationId Accommodation { get; }
        
        /// <summary>
        /// Reported duplicates.
        /// </summary>
        public List<ProviderAccommodationId> Duplicates { get; }
    }
}