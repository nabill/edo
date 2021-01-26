using System.Collections.Generic;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Availabilities.Mapping
{
    public readonly struct LocationDescriptor
    {
        public Location Location { get; init; }
        public Dictionary<Suppliers, List<SupplierCodeMapping>> AccommodationCodes { get; init; }
    }
}