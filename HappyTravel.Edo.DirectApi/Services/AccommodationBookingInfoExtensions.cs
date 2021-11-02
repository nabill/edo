using HappyTravel.Edo.DirectApi.Models;

namespace HappyTravel.Edo.DirectApi.Services
{
    public static class AccommodationBookingInfoExtensions
    {
        public static AccommodationBookingInfo FromEdoModel(this Edo.Api.Models.Bookings.AccommodationBookingInfo bookingInfo)
        {
            return new AccommodationBookingInfo(bookingId: bookingInfo.BookingId,
                bookingDetails: bookingInfo.BookingDetails.FromEdoModel(),
                counterpartyId: bookingInfo.CounterpartyId,
                paymentStatus: bookingInfo.PaymentStatus,
                totalPrice: bookingInfo.TotalPrice,
                supplier: bookingInfo.Supplier,
                agentInformation: bookingInfo.AgentInformation,
                paymentMethod: bookingInfo.PaymentMethod,
                tags: bookingInfo.Tags,
                isDirectContract: bookingInfo.IsDirectContract);
        }


        private static AccommodationBookingDetails FromEdoModel(this Edo.Api.Models.Bookings.AccommodationBookingDetails bookingDetails)
        {
            return new AccommodationBookingDetails();
        }
    }
}