using System.ComponentModel.DataAnnotations;

namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper
{
    public readonly struct CombineAccommodationsRequest
    {
        [Required]
        public string BaseHtAccommodationId { get; init; } 
        [Required]
        public string CombinedHtAccommodationId { get; init; }
    }
}