using System.Collections.Generic;
using HappyTravel.MapperContracts.Public.Accommodations.Enums;
using HappyTravel.SuppliersCatalog;

namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper
{
    public class AccommodationSearchRequest
    {
        public string NameQuery { get; init; }
        public int? CountryId { get; init; }
        public int? LocalityId { get; init; }
        public int? LocalityZoneId { get; init; }
        public bool? IsActive { get; init; }
        public List<AccommodationDeactivationReasons> DeactivationReasons { get; init; }
        public string AddressLineQuery { get; init; }
        public List<AccommodationRatings> Ratings { get; init; }
        public List<Suppliers> Suppliers { get; init; }
        public bool? HasDirectContract { get; init; }
        public int Skip { get; init; }
        public int Top { get; init; } = 100;
    }
}