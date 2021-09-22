using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper
{
    public readonly struct DeactivateAccommodationsRequest
    {
        [Required]
        public List<string> HtAccommodationIds { get; init; }
    }
}