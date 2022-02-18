using HappyTravel.MapperContracts.Public.Accommodations.Enums;

namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper
{
    public readonly struct SlimAccommodationData
    {
        public SlimAccommodationData(string htId, string name, string countryName, string localityName, string localityZoneName, string address, AccommodationRatings rating, bool isActive)
        {
            HtId = htId ?? string.Empty;
            Name = name ?? string.Empty;
            CountryName = countryName ?? string.Empty;
            LocalityName = localityName;
            LocalityZoneName = localityZoneName;
            Address = address ?? string.Empty;;
            Rating = rating;
            IsActive = isActive;
        }


        public string HtId { get; init; }
        public string Name { get; init; }
        public string CountryName { get; init; }
        public string LocalityName { get; init; }
        public string LocalityZoneName { get; init; }
        public string Address { get; init; }
        public AccommodationRatings Rating { get; init; }
        public bool IsActive { get; init; }
    }
}