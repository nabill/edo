using System.ComponentModel.DataAnnotations;
using HappyTravel.SuppliersCatalog;

namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper
{
    public struct RemoveSupplierRequest
    {
        [Required]
        public Suppliers Supplier { get; init; }
        
        [Required]
        public string SupplierId { get; init; }
    }
}