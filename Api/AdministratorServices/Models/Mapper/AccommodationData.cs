using System.Collections.Generic;
using HappyTravel.MapperContracts.Public.Accommodations.Enums;
using HappyTravel.MapperContracts.Public.Accommodations.Internals;

namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper
{
    public readonly struct AccommodationData
    {
        public AccommodationData(string name, string category, ContactInfo contacts, LocationInfo location, List<ImageInfo> photos, AccommodationRatings rating, 
            ScheduleInfo schedule, List<TextualDescription> textualDescriptions, PropertyTypes type, UniqueAccommodationCodes? uniqueCodes, string hotelChain, 
            List<string> accommodationAmenities, Dictionary<string, string> additionalInfo)
        {
            Name = name;
            Category = category;
            Contacts = contacts;
            Location = location;
            Photos = photos ?? new ();
            Rating = rating;
            Schedule = schedule;
            TextualDescriptions = textualDescriptions ?? new();
            Type = type;
            UniqueCodes = uniqueCodes ?? new ();
            HotelChain = hotelChain;
            AccommodationAmenities = accommodationAmenities ?? new ();
            AdditionalInfo = additionalInfo ?? new ();
        }
        
        
        public string Name { get; init; }
        public string Category { get; init; }
        public ContactInfo Contacts { get; init; }
        public LocationInfo Location { get; init; }
        public List<ImageInfo> Photos { get; init; }
        public AccommodationRatings Rating { get; init; }
        public ScheduleInfo Schedule { get; init; }
        public List<TextualDescription> TextualDescriptions { get; init; }
        public PropertyTypes Type { get; init; }
        public UniqueAccommodationCodes? UniqueCodes { get; init; }
        public string HotelChain { get; init; }
        public List<string> AccommodationAmenities { get; init; }
        public Dictionary<string, string> AdditionalInfo { get; init; }
    }
}