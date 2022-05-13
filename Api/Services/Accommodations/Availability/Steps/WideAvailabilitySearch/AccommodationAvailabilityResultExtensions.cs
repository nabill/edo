﻿using HappyTravel.Edo.Api.Models.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;

public static class AccommodationAvailabilityResultExtensions
{
    public static CachedAccommodationAvailabilityResult Map(this AccommodationAvailabilityResult result)
    {
        return new CachedAccommodationAvailabilityResult
        {
            SearchId = result.SearchId,
            SupplierCode = result.SupplierCode,
            Created = result.Created,
            AvailabilityId = result.AvailabilityId,
            RoomContractSets = result.RoomContractSets,
            MinPrice = result.MinPrice,
            MaxPrice = result.MaxPrice,
            CheckInDate = result.CheckInDate,
            CheckOutDate = result.CheckOutDate,
            HtId = result.HtId,
            SupplierAccommodationCode = result.SupplierAccommodationCode,
            CountryHtId = result.CountryHtId,
            LocalityHtId = result.LocalityHtId,
            MarketId = result.MarketId,
            CountryCode = result.CountryCode
        };
    }
}