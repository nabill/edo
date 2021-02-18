using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation
{
    public static class BookingEvaluationPriceProcessing
    {
        public static async ValueTask<EdoContracts.Accommodations.RoomContractSetAvailability?> ProcessPrices(EdoContracts.Accommodations.RoomContractSetAvailability? source,
            PriceProcessFunction processFunction)
        {
            if (source == null)
                return null;

            var value = source.Value;
            var roomContractSet = await RoomContractSetPriceProcessing.ProcessRoomContractSetPrice(value.RoomContractSet, processFunction);
            return new EdoContracts.Accommodations.RoomContractSetAvailability(value.AvailabilityId,
                value.CheckInDate,
                value.CheckOutDate,
                value.NumberOfNights,
                value.Accommodation,
                roomContractSet);
        }
        
        
        public static Currencies? GetCurrency(EdoContracts.Accommodations.RoomContractSetAvailability? availabilityDetails)
        {
            return availabilityDetails?.RoomContractSet.Rate.Currency;
        }
    }
}