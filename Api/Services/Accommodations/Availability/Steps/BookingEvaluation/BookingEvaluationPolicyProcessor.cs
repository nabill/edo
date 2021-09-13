using System;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation
{
    public static class BookingEvaluationPolicyProcessor
    {
        public static RoomContractSetAvailability? Process(RoomContractSetAvailability? roomContractSetAvailability, CancellationPolicyProcessSettings settings)
        {
            if (!roomContractSetAvailability.HasValue)
                return null;

            var availability = roomContractSetAvailability.Value;
            return new RoomContractSetAvailability(availabilityId: availability.AvailabilityId,
                accommodationId: availability.AccommodationId,
                checkInDate: availability.CheckInDate,
                checkOutDate: availability.CheckOutDate,
                numberOfNights: availability.NumberOfNights,
                roomContractSet: RoomContractSetPolicyProcessor.Process(availability.RoomContractSet, availability.CheckInDate,
                    settings),
                isCreditCardNeeded: availability.IsCreditCardNeeded);
        }
    }
}