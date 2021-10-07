using System.Collections.Generic;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.SuppliersCatalog;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation
{
    public static class RoomContractSetAvailabilityExtensions
    {
        public static RoomContractSetAvailability ToRoomContractSetAvailability(this in EdoContracts.Accommodations.RoomContractSetAvailability availabilityValue, Suppliers? supplier,
            bool isDirectContract, List<PaymentTypes> paymentMethods, SlimAccommodation accommodation)
        {
            return new RoomContractSetAvailability(availabilityId: availabilityValue.AvailabilityId,
                checkInDate: availabilityValue.CheckInDate,
                checkOutDate: availabilityValue.CheckOutDate,
                numberOfNights: availabilityValue.NumberOfNights,
                accommodation: accommodation,
                roomContractSet: availabilityValue.RoomContractSet.ToRoomContractSet(supplier, isDirectContract),
                availablePaymentMethods: paymentMethods);
        }
    }
}