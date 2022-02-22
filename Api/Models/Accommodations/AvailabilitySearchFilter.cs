#nullable enable

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using AccommodationRatings = HappyTravel.MapperContracts.Public.Accommodations.Enums.AccommodationRatings;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public record AvailabilitySearchFilter([Range(1, int.MaxValue)] int Top, 
        int Skip,
        decimal? MinPrice,
        decimal? MaxPrice,
        List<BoardBasisTypes>? BoardBasisTypes,
        List<AccommodationRatings>? Ratings,
        List<string>? Suppliers,
        string? Order,
        string? Direction = "asc");
}