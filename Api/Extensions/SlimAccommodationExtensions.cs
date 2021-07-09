using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Geography;
using HappyTravel.MapperContracts.Public.Accommodations.Internals;
using GeoPoint = HappyTravel.Geography.GeoPoint;
using SlimLocationInfo = HappyTravel.Edo.Api.Models.Accommodations.SlimLocationInfo;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class SlimAccommodationExtensions
    {
        public static SlimAccommodation ToEdoContract(this MapperContracts.Public.Accommodations.SlimAccommodation accommodation) 
            => new SlimAccommodation(location: accommodation.Location.Map(),
                name: accommodation.Name,
                photo: new ImageInfo(accommodation.Photo.SourceUrl, accommodation.Photo.Caption),
                rating: accommodation.Rating,
                propertyType: accommodation.PropertyType,
                htId: accommodation.HtId,
                hotelChain: accommodation.HotelChain);


        private static SlimLocationInfo Map(this MapperContracts.Public.Accommodations.Internals.SlimLocationInfo locationInfo) 
            => new (address: locationInfo.Address,
                country: locationInfo.Country,
                countryCode: locationInfo.CountryCode,
                locality: locationInfo.Locality,
                localityZone: locationInfo.LocalityZone,
                coordinates: new GeoPoint(locationInfo.Coordinates.Longitude, locationInfo.Coordinates.Latitude));
    }
}