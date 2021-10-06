using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Api.Models.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public static class WideAvailabilityPolicyProcessor
    {
        public static List<AccommodationAvailabilityResult> Process(List<AccommodationAvailabilityResult> results, CancellationPolicyProcessSettings settings)
        {
            return results
                .Select(r =>
                {
                    return new AccommodationAvailabilityResult(searchId: r.SearchId,
                        supplier: r.Supplier,
                        created: r.Created,
                        availabilityId: r.AvailabilityId,
                        roomContractSets: RoomContractSetPolicyProcessor_New.Process(r.RoomContractSets, r.CheckInDate, settings),
                        minPrice: r.MinPrice,
                        maxPrice: r.MaxPrice,
                        checkInDate: r.CheckInDate,
                        checkOutDate: r.CheckOutDate,
                        htId: r.HtId,
                        supplierAccommodationCode: r.SupplierAccommodationCode);
                })
                .ToList();
        }
    }
}