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
            return new RoomContractSetAvailability(availability.AvailabilityId,
                availability.CheckInDate,
                availability.CheckOutDate,
                availability.NumberOfNights,
                availability.Accommodation,
                RoomContractSetPolicyProcessor.Process(availability.RoomContractSet, availability.CheckInDate,
                    settings));
        }
    }
}