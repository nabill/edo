using System.Linq;
using HappyTravel.Edo.DirectApi.Models;

namespace HappyTravel.Edo.DirectApi.Services
{
    public static class AccommodationBookingRequestExtensions
    {
        public static Edo.Api.Models.Bookings.AccommodationBookingRequest ToEdoModel(this AccommodationBookingRequest request)
        {
            return new Edo.Api.Models.Bookings.AccommodationBookingRequest(itineraryNumber: request.Nationality,
                nationality: request.Nationality,
                residency: request.Residency,
                clientReferenceCode: request.ReferenceCode,
                roomDetails: request.RoomDetails
                    .Select(r => r.ToEdoModel())
                    .ToList(),
                features: request.Features
                    .Select(f => f.ToEdoModel())
                    .ToList(),
                searchId: request.SearchId,
                htId: request.HtId,
                roomContractSetId: request.RoomContractSetId,
                mainPassengerName: request.MainPassengerName,
                evaluationToken: request.EvaluationToken,
                rejectIfUnavailable: request.RejectIfUnavailable);
        }


        private static Edo.Api.Models.Bookings.BookingRoomDetails ToEdoModel(this BookingRoomDetails roomDetails)
        {
            return new Api.Models.Bookings.BookingRoomDetails(type: roomDetails.Type, 
                passengers: roomDetails.Passengers,
                isExtraBedNeeded: roomDetails.IsExtraBedNeeded,
                isCotNeeded: roomDetails.IsCotNeededNeeded);
        }


        private static Edo.Api.Models.Accommodations.AccommodationFeature ToEdoModel(this AccommodationFeature accommodationFeature)
        {
            return new Api.Models.Accommodations.AccommodationFeature(accommodationFeature.Type, accommodationFeature.Value);
        }
    }
}