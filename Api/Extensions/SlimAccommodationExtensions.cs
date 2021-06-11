using System;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.Accommodations.Internals;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class SlimAccommodationExtensions
    {
        public static SlimAccommodation ToEdoContract(this MapperContracts.Public.Accommodations.SlimAccommodation accommodation) 
            => new (id: accommodation.HtId,
                location: accommodation.Location.Map(),
                name: accommodation.Name,
                photo: new ImageInfo(accommodation.Photo.SourceUrl, accommodation.Photo.Caption),
                rating: accommodation.Rating.Map<AccommodationRatings, MapperContracts.Public.Accommodations.Enums.AccommodationRatings>(),
                propertyType: accommodation.PropertyType.Map<PropertyTypes, MapperContracts.Public.Accommodations.Enums.PropertyTypes>(),
                htId: accommodation.HtId,
                hotelChain: accommodation.HotelChain);


        private static T1 Map<T1, T2>(this T2 value) where T1 : struct, Enum where T2 : struct, Enum
            => Enum.Parse<T1>(value.ToString());


        private static SlimLocationInfo Map(this MapperContracts.Public.Accommodations.Internals.SlimLocationInfo locationInfo) 
            => new(address: locationInfo.Address,
                country: locationInfo.Country,
                countryCode: locationInfo.CountryCode,
                locality: locationInfo.Locality,
                localityZone: locationInfo.LocalityZone,
                coordinates: locationInfo.Coordinates);
    }
}