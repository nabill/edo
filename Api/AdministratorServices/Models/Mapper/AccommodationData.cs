using System.Collections.Generic;
using HappyTravel.MapperContracts.Public.Accommodations.Enums;
using HappyTravel.MapperContracts.Public.Accommodations.Internals;

namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper
{
    public class AccommodationData
    {
        public string Name { get; init; }
        public string Category { get; init; }
        public ContactInfo Contacts { get; init; }
        public LocationInfo Location { get; init; }
        public List<ImageInfo> Photos { get; init; } = new ();
        public AccommodationRatings Rating { get; init; }
        public ScheduleInfo Schedule { get; init; }
        public List<TextualDescription> TextualDescriptions { get; init; } = new ();
        public PropertyTypes Type { get; init; }
        public UniqueAccommodationCodes? UniqueCodes { get; init; }
        public string HotelChain { get; init; }
        public List<string> AccommodationAmenities { get; init; } = new ();
        public Dictionary<string, string> AdditionalInfo { get; init; } = new ();
    }
}