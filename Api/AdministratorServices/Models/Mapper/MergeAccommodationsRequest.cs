using System.ComponentModel.DataAnnotations;

namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper
{
    public readonly struct MergeAccommodationsRequest
    {
        [Required]
        public string BaseHtAccommodationId { get; init; } 
        
        [Required]
        public string HtAccommodationIdToMerge { get; init; }
    }
}