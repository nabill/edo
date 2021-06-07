using System.Collections.Generic;
using HappyTravel.MapperContracts.Internal.Mappings.Internals;
using HappyTravel.SuppliersCatalog;

namespace HappyTravel.Edo.Api.Models.Availabilities.Mapping
{
    public readonly struct SearchArea
    {
        public List<Location> Locations { get; init; }
        public Dictionary<Suppliers, List<SupplierCodeMapping>> AccommodationCodes { get; init; }
    }
}