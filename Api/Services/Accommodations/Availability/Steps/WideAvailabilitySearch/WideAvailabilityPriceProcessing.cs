using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public static class WideAvailabilityPriceProcessing
    {
        public static Currencies? GetCurrency(SlimAccommodationAvailability accommodationAvailability)
        {
            if (!accommodationAvailability.RoomContractSets.Any())
                return null;

            return accommodationAvailability.RoomContractSets.First().Rate.Currency;
        }
        

        public static async ValueTask<SlimAccommodationAvailability> ProcessPrices(SlimAccommodationAvailability supplierResponse, PriceProcessFunction function)
        {
            var supplierRoomContractSets = supplierResponse.RoomContractSets;
            var roomContractSetsWithMarkup = await RoomContractSetPriceProcessing.ProcessRoomContractSetsPrices(supplierRoomContractSets, function);
            var convertedAccommodationAvailability = new SlimAccommodationAvailability(supplierResponse.Accommodation,
                roomContractSetsWithMarkup,
                supplierResponse.AvailabilityId);
            
            return convertedAccommodationAvailability;
        }
    }
}