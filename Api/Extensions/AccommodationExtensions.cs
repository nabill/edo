using System;
using System.Linq;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.Accommodations.Internals;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class AccommodationExtensions
    {
        public static Accommodation ToEdoContract(this MapperContracts.Public.Accommodations.Accommodation accommodation) 
            => new(id: accommodation.HtId,
                name: accommodation.Name,
                accommodationAmenities: accommodation.AccommodationAmenities,
                additionalInfo: accommodation.AdditionalInfo,
                category: accommodation.Category,
                contacts: new ContactInfo(emails: accommodation.Contacts.Emails,
                    phones: accommodation.Contacts.Phones,
                    webSites: accommodation.Contacts.WebSites,
                    faxes: accommodation.Contacts.Faxes),
                location: new LocationInfo(countryCode: accommodation.Location.CountryCode,
                    countryHtId: accommodation.Location.CountryHtId,
                    localityHtId: accommodation.Location.LocalityHtId,
                    locality: accommodation.Location.Locality,
                    country: accommodation.Location.Country,
                    localityZoneHtId: accommodation.Location.LocalityZoneHtId,
                    coordinates: accommodation.Location.Coordinates,
                    localityZone: accommodation.Location.LocalityZone,
                    address: accommodation.Location.Address,
                    locationDescriptionCode: accommodation.Location.LocationDescriptionCode.Map<LocationDescriptionCodes, MapperContracts.Public.Accommodations.Enums.LocationDescriptionCodes>(), 
                    pointsOfInterests: accommodation.Location.PointsOfInterests.Select(p => new PoiInfo(name: p.Name,
                        distance: p.Distance,
                        time: p.Time,
                        type: p.Type.Map<PoiTypes, MapperContracts.Public.Accommodations.Enums.PoiTypes>())).ToList(),
                    isHistoricalBuilding: accommodation.Location.IsHistoricalBuilding),
                photos: accommodation.Photos.Select(p => new ImageInfo(p.SourceUrl, p.Caption)).ToList(),
                rating: accommodation.Rating.Map<AccommodationRatings, MapperContracts.Public.Accommodations.Enums.AccommodationRatings>(),
                schedule: new ScheduleInfo(checkInTime: accommodation.Schedule.CheckInTime,
                    portersStartTime: accommodation.Schedule.PortersStartTime,
                    portersEndTime: accommodation.Schedule.PortersEndTime,
                    checkOutTime: accommodation.Schedule.CheckOutTime,
                    roomServiceStartTime: accommodation.Schedule.RoomServiceStartTime,
                    roomServiceEndTime: accommodation.Schedule.RoomServiceEndTime),
                textualDescriptions: accommodation.TextualDescriptions.Select(t => new TextualDescription(t.Type.Map<TextualDescriptionTypes, MapperContracts.Public.Accommodations.Enums.TextualDescriptionTypes>(), t.Description)).ToList(),
                type: accommodation.Type.Map<PropertyTypes, MapperContracts.Public.Accommodations.Enums.PropertyTypes>(),
                htId: accommodation.HtId,
                uniqueCodes: new UniqueAccommodationCodes(accommodation.UniqueCodes?.GiataId, accommodation.UniqueCodes?.SynxisId),
                hotelChain: accommodation.HotelChain,
                modified: accommodation.Modified
            );
        
        
        private static T1 Map<T1, T2>(this T2 value) where T1 : struct, Enum where T2 : struct, Enum
            => Enum.Parse<T1>(value.ToString());
    }
}