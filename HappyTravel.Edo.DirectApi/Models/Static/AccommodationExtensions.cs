using System.Collections.Generic;
using System.Linq;

namespace HappyTravel.Edo.DirectApi.Models.Static
{
    public static class AccommodationExtensions
    {
        public static Accommodation ToDirectApiModel(this MapperContracts.Public.Accommodations.Accommodation accommodation)
        {
            return new Accommodation(id: accommodation.HtId,
                name: accommodation.Name,
                accommodationAmenities: accommodation.AccommodationAmenities,
                additionalInfo: accommodation.AdditionalInfo,
                category: accommodation.Category,
                contacts: new ContactInfo(emails: accommodation.Contacts.Emails, 
                    phones: accommodation.Contacts.Phones, 
                    webSites: accommodation.Contacts.WebSites, 
                    faxes: accommodation.Contacts.Faxes),
                location: new LocationInfo(countryCode: accommodation.Location.CountryCode,
                    countryId: accommodation.Location.CountryHtId,
                    country: accommodation.Location.Country,
                    localityId: accommodation.Location.LocalityHtId,
                    locality: accommodation.Location.Locality,
                    localityZoneId: accommodation.Location.LocalityZoneHtId,
                    localityZone: accommodation.Location.LocalityZone,
                    coordinates: new GeoPoint(accommodation.Location.Coordinates.Longitude, accommodation.Location.Coordinates.Latitude),
                    address: accommodation.Location.Address,
                    locationDescriptionCode: accommodation.Location.LocationDescriptionCode,
                    pointsOfInterests: accommodation.Location.PointsOfInterests
                        .Select(p => new PoiInfo(name: p.Name,
                            distance: p.Distance,
                            time: p.Time,
                            type: p.Type,
                            description: p.Description))
                        .ToList()),
                photos: accommodation.Photos
                    .Select(p => new ImageInfo(p.SourceUrl, p.Caption))
                    .ToList(),
                rating: accommodation.Rating,
                schedule: new ScheduleInfo(checkInTime: accommodation.Schedule.CheckInTime,
                    checkOutTime: accommodation.Schedule.CheckOutTime,
                    portersStartTime: accommodation.Schedule.PortersStartTime,
                    portersEndTime: accommodation.Schedule.PortersEndTime,
                    roomServiceStartTime: accommodation.Schedule.RoomServiceStartTime,
                    roomServiceEndTime: accommodation.Schedule.RoomServiceEndTime),
                textualDescriptions: accommodation.TextualDescriptions
                    .Select(d => new TextualDescription(d.Type, d.Description))
                    .ToList(),
                type: accommodation.Type,
                hotelChain: accommodation.HotelChain,
                modified: accommodation.Modified);
        }


        public static List<Accommodation> ToDirectApiModels(this List<MapperContracts.Public.Accommodations.Accommodation> accommodations) 
            => accommodations.Select(ToDirectApiModel).ToList();
    }
}