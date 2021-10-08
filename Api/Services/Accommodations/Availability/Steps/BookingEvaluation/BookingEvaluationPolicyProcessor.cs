using HappyTravel.Edo.Api.Models.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation
{
    public static class BookingEvaluationPolicyProcessor
    {
        public static RoomContractSetAvailability Process(RoomContractSetAvailability availability, CancellationPolicyProcessSettings settings)
        {
            return new RoomContractSetAvailability(availabilityId: availability.AvailabilityId,
                checkInDate: availability.CheckInDate,
                checkOutDate: availability.CheckOutDate,
                numberOfNights: availability.NumberOfNights,
                roomContractSet: RoomContractSetPolicyProcessor_New.Process(availability.RoomContractSet, availability.CheckInDate,
                    settings),
                availablePaymentMethods: availability.AvailablePaymentMethods,
                accommodation: availability.Accommodation);
        }
    }
}