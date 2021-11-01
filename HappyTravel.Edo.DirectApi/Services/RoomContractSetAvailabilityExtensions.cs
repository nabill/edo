using HappyTravel.Edo.DirectApi.Models;

namespace HappyTravel.Edo.DirectApi.Services
{
    public static class RoomContractSetAvailabilityExtensions
    {
        public static RoomContractSetAvailability MapFromEdoModels(this Api.Models.Accommodations.RoomContractSetAvailability availability)
        {
            return new RoomContractSetAvailability(availabilityId: availability.AvailabilityId,
                checkInDate: availability.CheckInDate,
                checkOutDate: availability.CheckOutDate,
                numberOfNights: availability.NumberOfNights,
                roomContractSet: availability.RoomContractSet.MapFromEdoModel(),
                availablePaymentMethods: availability.AvailablePaymentMethods,
                countryHtId: availability.CountryHtId,
                localityHtId: availability.LocalityHtId);
        }
    }
}