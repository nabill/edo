using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection
{
    public static class RoomSelectionPriceProcessing
    {
        public static async ValueTask<AccommodationAvailability> ProcessPrices(AccommodationAvailability source,
            PriceProcessFunction processFunction)
        {
            var roomContractSets = await RoomContractSetPriceProcessing.ProcessPrices(source.RoomContractSets, processFunction);
            return new AccommodationAvailability(source.AvailabilityId,
                source.CheckInDate,
                source.CheckOutDate,
                source.NumberOfNights,
                source.Accommodation,
                roomContractSets);
        }
        
        
        public static Currencies? GetCurrency(AccommodationAvailability availabilityDetails)
        {
            if (!availabilityDetails.RoomContractSets.Any())
                return null;
            
            return availabilityDetails.RoomContractSets
                .Select(a => a.Rate.Currency)
                .First();
        }
    }
}