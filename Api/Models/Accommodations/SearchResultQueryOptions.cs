#nullable enable

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.SuppliersCatalog;
using AccommodationRatings = HappyTravel.MapperContracts.Public.Accommodations.Enums.AccommodationRatings;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public record SearchResultQueryOptions([Range(1, int.MaxValue)] int Top, 
        int Skip,
        decimal? MinPrice,
        decimal? MaxPrice,
        List<BoardBasisTypes>? BoardBasisTypes,
        List<AccommodationRatings>? Ratings,
        List<Suppliers>? Suppliers);
}