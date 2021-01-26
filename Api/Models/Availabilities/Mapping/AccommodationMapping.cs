using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Availabilities.Mapping
{
    public readonly struct AccommodationMapping
    {
        public string HtId { get; init; }
        public Dictionary<Suppliers, string[]> SupplierCodes { get; init; }
    }
}