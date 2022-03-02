using System.Collections.Generic;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation
{
    public static class RoomContractSetAvailabilityExtensions
    {
        public static RoomContractSetAvailability ToRoomContractSetAvailability(
            this in EdoContracts.Accommodations.RoomContractSetAvailability availabilityValue, string supplier, string supplierCode,
            List<PaymentTypes> paymentMethods, SlimAccommodation accommodation, string countryHtId, string localityHtId, string evaluationToken)
        {
            return new RoomContractSetAvailability(availabilityId: availabilityValue.AvailabilityId,
                checkInDate: availabilityValue.CheckInDate,
                checkOutDate: availabilityValue.CheckOutDate,
                numberOfNights: availabilityValue.NumberOfNights,
                accommodation: accommodation,
                roomContractSet: availabilityValue.RoomContractSet.ToRoomContractSet(supplier, supplierCode, availabilityValue.RoomContractSet.IsDirectContract),
                availablePaymentMethods: paymentMethods,
                countryHtId: countryHtId,
                localityHtId: localityHtId,
                evaluationToken: evaluationToken);
        }
    }
}