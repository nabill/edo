using System.ComponentModel.DataAnnotations;

namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper
{
    public readonly struct RemoveSupplierRequest
    {
        [Required]
        public string SupplierId { get; init; }
    }
}