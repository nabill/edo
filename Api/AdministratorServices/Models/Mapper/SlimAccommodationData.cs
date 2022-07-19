using HappyTravel.MapperContracts.Public.Accommodations.Enums;

namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper
{
    public record SlimAccommodationData
    {
        public string HtId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string CountryName { get; init; } = string.Empty;
        public string? LocalityName { get; init; } = string.Empty;
        public string? LocalityZoneName { get; init; }
        public string Address { get; init; } = string.Empty;
        public AccommodationRatings Rating { get; init; }
        public bool IsActive { get; init; }
    }
}