using HappyTravel.Edo.Api.Models.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;

public static class CachedAccommodationAvailabilityResultExtensions
{
    public static AccommodationAvailabilityResult Map(this CachedAccommodationAvailabilityResult result)
    {
        return new AccommodationAvailabilityResult(searchId: result.SearchId,
            supplierCode: result.SupplierCode,
            created: result.Created,
            availabilityId: result.AvailabilityId,
            roomContractSets: result.RoomContractSets,
            minPrice: result.MinPrice,
            maxPrice: result.MaxPrice,
            checkInDate: result.CheckInDate,
            checkOutDate: result.CheckOutDate,
            htId: result.HtId,
            supplierAccommodationCode: result.SupplierAccommodationCode,
            countryHtId: result.CountryHtId,
            localityHtId: result.LocalityHtId,
            marketId: result.MarketId,
            countryCode: result.CountryCode);
    }
}