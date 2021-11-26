using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.DirectApi.Models;

namespace HappyTravel.Edo.DirectApi.Services.Bookings
{
    public static class AccommodationBookingRequestExtensions
    {
        public static Edo.Api.Models.Bookings.AccommodationBookingRequest ToEdoModel(this AccommodationBookingRequest request)
        {
            return new Edo.Api.Models.Bookings.AccommodationBookingRequest(itineraryNumber: string.Empty,
                nationality: request.Nationality,
                residency: request.Residency,
                clientReferenceCode: request.ReferenceCode,
                roomDetails: request.RoomDetails
                    .Select(r => r.ToEdoModel())
                    .ToList(),
                features: new List<Api.Models.Accommodations.AccommodationFeature>(),
                searchId: request.SearchId,
                htId: request.AccommodationId,
                roomContractSetId: request.RoomContractSetId,
                mainPassengerName: GetMainPassengerName(request.RoomDetails),
                evaluationToken: null,
                rejectIfUnavailable: true);
        }


        private static Edo.Api.Models.Bookings.BookingRoomDetails ToEdoModel(this BookingRoomDetails roomDetails)
        {
            return new Api.Models.Bookings.BookingRoomDetails(type: roomDetails.Type, 
                passengers: roomDetails.Passengers,
                isExtraBedNeeded: roomDetails.IsExtraBedNeeded,
                isCotNeeded: roomDetails.IsCotNeededNeeded);
        }


        private static string GetMainPassengerName(IEnumerable<BookingRoomDetails> roomDetail)
        {
            return roomDetail.SelectMany(rd => rd.Passengers)
                .Where(p => p.IsLeader)
                .Select(p => $"{p.FirstName} {p.LastName}")
                .Single();
        }
    }
}