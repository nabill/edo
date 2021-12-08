using System.Collections.Generic;
using HappyTravel.MapperContracts.Public.Accommodations.Enums;
using HappyTravel.SuppliersCatalog;

namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper
{
    public readonly struct AccommodationSearchRequest
    {
        public AccommodationSearchRequest(string nameQuery, int? countryId, int? localityId, int? localityZoneId, bool? isActive, List<AccommodationDeactivationReasons> deactivationReasons, string addressLineQuery, List<AccommodationRatings> ratings, List<Suppliers> suppliers, bool? hasDirectContract, int? skip, int? top)
        {
            NameQuery = nameQuery;
            CountryId = countryId;
            LocalityId = localityId;
            LocalityZoneId = localityZoneId;
            IsActive = isActive;
            DeactivationReasons = deactivationReasons ?? new();
            AddressLineQuery = addressLineQuery;
            Ratings = ratings ?? new();
            Suppliers = suppliers ?? new ();
            HasDirectContract = hasDirectContract;
            Skip = skip ?? default;
            Top = top ?? 100;
        }


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
        public int Top { get; init; }
    }
}