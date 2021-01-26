using System.Collections.Generic;
using HappyTravel.Edo.Api.Models.Locations;

namespace HappyTravel.Edo.Api.Models.Availabilities.Mapping
{
    public readonly struct LocationMapping
    {
        public Location Location { get; init; }
        public List<AccommodationMapping> AccommodationMappings { get; init; }
    }
}