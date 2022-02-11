using System.Collections.Generic;
using HappyTravel.MapperContracts.Internal.Mappings.Internals;

namespace HappyTravel.Edo.Api.Models.Availabilities.Mapping
{
    public readonly struct SearchArea
    {
        public List<Location> Locations { get; init; }
        public Dictionary<string, List<SupplierCodeMapping>> AccommodationCodes { get; init; }
    }
}