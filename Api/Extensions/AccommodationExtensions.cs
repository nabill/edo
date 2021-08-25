using System.Linq;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Geography;
using HappyTravel.MapperContracts.Public.Accommodations.Internals;
using ContactInfo = HappyTravel.Edo.Api.Models.Accommodations.ContactInfo;
using GeoPoint = HappyTravel.Geography.GeoPoint;
using LocationInfo = HappyTravel.Edo.Api.Models.Accommodations.LocationInfo;

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
                    coordinates: new GeoPoint(accommodation.Location.Coordinates.Longitude,accommodation.Location.Coordinates.Latitude),
                    localityZone: accommodation.Location.LocalityZone,
                    address: accommodation.Location.Address,
                    locationDescriptionCode: accommodation.Location.LocationDescriptionCode, 
                    pointsOfInterests: accommodation.Location.PointsOfInterests.Select(p => new PoiInfo(name: p.Name,
                        distance: p.Distance,
                        time: p.Time,
                        type: p.Type)).ToList(),
                    isHistoricalBuilding: accommodation.Location.IsHistoricalBuilding),
                photos: accommodation.Photos.Select(p => new ImageInfo(p.SourceUrl, p.Caption)).ToList(),
                rating: accommodation.Rating,
                schedule: new ScheduleInfo(checkInTime: accommodation.Schedule.CheckInTime,
                    portersStartTime: accommodation.Schedule.PortersStartTime,
                    portersEndTime: accommodation.Schedule.PortersEndTime,
                    checkOutTime: accommodation.Schedule.CheckOutTime,
                    roomServiceStartTime: accommodation.Schedule.RoomServiceStartTime,
                    roomServiceEndTime: accommodation.Schedule.RoomServiceEndTime),
                textualDescriptions: accommodation.TextualDescriptions,
                type: accommodation.Type,
                htId: accommodation.HtId,
                uniqueCodes: new UniqueAccommodationCodes(accommodation.UniqueCodes?.GiataId, accommodation.UniqueCodes?.SynxisId),
                hotelChain: accommodation.HotelChain,
                modified: accommodation.Modified
            );
    }
}