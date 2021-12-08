using System.ComponentModel.DataAnnotations;

namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper
{
    public readonly struct DeactivateAccommodationDescriptionRequest
    {
        [Required]
        public string DeactivationReasonDescription{ get; init; }
    }
}


