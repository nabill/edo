using System.Collections.Generic;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Availabilities.Mapping
{
    public readonly struct SearchArea
    {
        public List<Location> Locations { get; init; }
        public Dictionary<Suppliers, List<SupplierCodeMapping>> AccommodationCodes { get; init; }
    }
}