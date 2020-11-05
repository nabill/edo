using System.Collections.Generic;
using HappyTravel.Edo.Data.AccommodationMappings;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct ReportAccommodationDuplicateRequest
    {
        [JsonConstructor]
        public ReportAccommodationDuplicateRequest(SupplierAccommodationId accommodation, List<SupplierAccommodationId> duplicates)
        {
            Accommodation = accommodation;
            Duplicates = duplicates;
        }
        
        /// <summary>
        /// Reported accommodation.
        /// </summary>
        public SupplierAccommodationId Accommodation { get; }
        
        /// <summary>
        /// Reported duplicates.
        /// </summary>
        public List<SupplierAccommodationId> Duplicates { get; }
    }
}